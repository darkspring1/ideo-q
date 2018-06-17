using Converter.DAL;
using Microsoft.Extensions.Logging;
using Converter.Settings;
using System.Linq;
using Converter.DAL.Entity;
using Microsoft.Extensions.Caching.Memory;
using Converter.DAL.Constants;
using Converter.Size.Converters;

namespace Converter.Size
{
    class SizeConvertStrategy : BaseConvertStrategy<SizeConverterSettings>
    {
        const string FSIZE_CACHE_KEY_TEMPLATE = "fsize_{0}";
        private readonly IMemoryCache _cache;
       
        private readonly DirectMappingConverter _directMappingConverter;
        private readonly SeparateBySlashConverter _separateBySlashConverter;

        public SizeConvertStrategy(
            Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings,
            IMemoryCache cache) : base(Taxonomy.PA_FSIZE, dao, logger, settings)
        {
            _cache = cache;
            _directMappingConverter = new DirectMappingConverter(Settings.DirectMapping);
            _separateBySlashConverter = new SeparateBySlashConverter();
        }

        protected override void BeforeExecute()
        {
            LoadExistedFSizesToCache();
        }

        protected override void AfterSave()
        {
            //проставим количество
            var taxonomyWithCounts = Dao.GetFSizeTaxonomyCount();
            var _fsizes = Dao.GetFSizes();
            foreach (var fsize in _fsizes)
            {
                fsize.count = taxonomyWithCounts[fsize.term_taxonomy_id];
            }

            Dao.SaveChanges();
        }

        string[] GetFSizesFromSettings()
        {
            return Settings
                .SizeCharts
                .SelectMany(x => x)
                .SelectMany(x => x)
                .Select(x => $"{x.Key}{x.Value}".ToLower())
                .Distinct()
                .ToArray();
        }

        /// <summary>
        /// существующие размеры в кэш
        /// </summary>
        void LoadExistedFSizesToCache()
        {
            var fsizes = Dao.GetFSizes(true);

            foreach (var fsize in fsizes)
            {
                _cache.Set(string.Format(FSIZE_CACHE_KEY_TEMPLATE, fsize.TermName), fsize);
            }
        }

        ISizeChart GetSizeChart(TermTaxonomy[] cats)
        {
            foreach (var cat in cats)
            {
                var categoryName = cat.Term.LowerName;
                var key = $"sizeChart_{categoryName}";
                var sizeChart = _cache.GetOrCreate(key, cacheEntry =>
                {
                    var binding = Settings.SizeChartBindings.FirstOrDefault(x => x.Categories.Contains(categoryName));
                    if (binding == null)
                    {
                        return null;
                    }
                    return Settings.SizeCharts.First(sc => sc.Name == binding.SizeChartName);
                });

                if (sizeChart != null)
                {
                    return sizeChart;
                }
            }
            return null;
        }

        TermTaxonomy GetFSize(string fsizeName)
        {
            return _cache.GetOrCreate(string.Format(FSIZE_CACHE_KEY_TEMPLATE, fsizeName), cacheEntry =>
            {
                return Dao.CreateFSize(fsizeName);
            });
        }
        
        protected override void ConverPost(Post post)
        {
            if (post.Sizes.Any())
            {
                if (post.ID == 7021)
                {

                }
                if (post.Categories.Any())
                {
                    var sizeChart = GetSizeChart(post.Categories);
                    ConvertersChain converters = new ConvertersChain();
                    //порядок в котором добавлются конвертеры важен!!
                    converters.Add(_directMappingConverter);
                    
                    if (sizeChart == null)
                    {
                        Logger.LogInformation($"Post {post.ID} has no size chart");
                    }
                    else
                    {
                        converters.Add(new SizeChartConverter(sizeChart, _cache));
                    }

                    converters.Add(_separateBySlashConverter);
                    //converters.Add(_asIsConverter);

                    foreach (var size in post.Sizes)
                    {
                        if (Settings.Ignore.Any(x => x == size.TermName))
                        {
                            WriteToResultFile($"postId: {post.ID}, {size.TermName} => IGNORED");
                        }
                        else
                        {
                            string[] fsizeNames = converters.Convert(size.TermName);
                            bool wasConverted = true;
                            if (!fsizeNames.Any())
                            {
                                fsizeNames = new[] { size.TermName.TryWithoutPrefix() };
                                wasConverted = false;
                            }

                            var setFSizeNames = "";
                            foreach (var fsizeName in fsizeNames)
                            {
                                var fsize = GetFSize(fsizeName);
                                if (post.SetFSize(fsize))
                                {
                                    setFSizeNames += $"{fsizeName},";
                                }
                                else
                                {
                                    Logger.LogInformation($"fsize {fsize.TermName} already set for post {post.ID}.");
                                }
                            }
                            if (setFSizeNames != "")
                            {
                                WriteToResultFile($"postId: {post.ID}, {size.TermName} => {setFSizeNames.TrimEnd(',')}");
                            }
                            if (!wasConverted)
                            {
                                var categoriesStr = string.Join(",", post.Categories.Select(x => x.TermName));
                                var sizeChartName = sizeChart?.Name ?? "not found";
                                WriteToUnknownFile($"PostId:{post.ID}, TermId:{size.term_id}, TermName:{size.TermName}, SizeChart: {sizeChartName}, Categories: {categoriesStr}");
                            }
                        }
                    }
                }
                else
                {
                    Logger.LogInformation($"Post {post.ID} has no categories");
                }
            }
            else
            {
                Logger.LogInformation($"Post {post.ID} has no sizes");
            }
        }


    }
}
