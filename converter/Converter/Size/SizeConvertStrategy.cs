using Converter.DAL;
using Microsoft.Extensions.Logging;
using Converter.Settings;
using System.Linq;
using Converter.DAL.Entity;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Converter.Size
{
    class SizeConvertStrategy : BaseConvertStrategy<SizeConverterSettings>
    {
        const string FSIZE_CACHE_KEY_TEMPLATE = "fsize_{0}";
        private readonly IMemoryCache _cache;
        private readonly SizeConverter _converter;

        public SizeConvertStrategy(
            Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings,
            IMemoryCache cache) : base(dao, logger, settings)
        {
            _cache = cache;
            _converter = new SizeConverter(Settings.DirectMapping, _cache);
        }

        protected override void BeforeExecute()
        {
            LoadExistedFSizesToCache();
        }

        protected override void AfterSave()
        {
            
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
                _cache.Set(string.Format(FSIZE_CACHE_KEY_TEMPLATE, fsize.Term.LowerName), fsize);
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
                if (post.Categories.Any())
                {
                    var sizeChart = GetSizeChart(post.Categories);
                    if (sizeChart == null)
                    {
                        Logger.LogInformation($"Post {post.ID} has no size chart");
                    }
                  
                    foreach (var size in post.Sizes)
                    {
                        bool wasConverted = false;
                        var fsizeNames = _converter.Convert(sizeChart, size.TermName, out wasConverted);
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
