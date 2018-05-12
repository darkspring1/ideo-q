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
        
        public SizeConvertStrategy(
            Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings,
            IMemoryCache cache) : base(dao, logger, settings)
        {
            
            _cache = cache;
        }

        public void Execute()
        {
            //UpdateFColours();
            LoadExistedFSizesToCache();
            var posts = Dao.GetPosts();

            foreach (var p in posts)
            {
                ConverPost(p/*, colorConverter*/);
            }
            
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

        /*
        void UpdateFColours()
        {
            var fsizes = GetFSizesFromSettings();
            
            if (fsizes.Any())
            {
                var existedFColours = dao
                    .GetFilterableColours(true);
                var existedFColoursNames = existedFColours.Select(x => x.Term.LowerName).ToArray();

                var forDelete = existedFColours
                    .Where(x => !_settings.FColours.Contains(x.Term.LowerName))
                    .ToArray();

                var forAdd = _settings
                    .FColours
                    .Where(x => !existedFColoursNames.Contains(x))
                    .ToArray();

                dao.DeleteFColours(forDelete);
                dao.CreateFColours(forAdd);
                dao.SaveChanges();

                if (forDelete.Any())
                {
                    _logger.LogInformation($"fcolours were deleted: {string.Join(",", forDelete.Select(x => x.Term.LowerName))}");
                }
                if (forAdd.Any())
                {
                    _logger.LogInformation($"fcolours were added: {string.Join(",", forAdd)}");
                }
            }

        }
        */

        SizeChart GetSizeChart(TermTaxonomy[] cats)
        {
            foreach (var cat in cats)
            {
                var categoryName = cat.Term.LowerName;
                var key = $"sizeChart_{categoryName}";
                var sizeChart = _cache.GetOrCreate(key, cacheEntry =>
                {
                    //cacheEntry.

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
        
        void ConverPost(Post post/*, ColorConverter converter*/)
        {
            if (post.Sizes.Any())
            {
                if (post.Categories.Any())
                {
                    var sizeChart = GetSizeChart(post.Categories);
                    var converter = new SizeConverter(sizeChart);
                    Func<TermTaxonomy, string[]> convertFunc;
                    if (sizeChart != null)
                    {
                        convertFunc = size => converter.Convert(size.Term.LowerName);
                    }
                    else
                    {
                        Logger.LogInformation($"Post {post.ID} has no size chart");
                        convertFunc = size => new[] { size.Term.LowerName };
                    }

                    foreach (var size in post.Sizes)
                    {
                        var fsizeNames = convertFunc(size);
                        foreach (var fsizeName in fsizeNames)
                        {
                            var fsize = GetFSize(fsizeName);
                            if (post.SetFSize(fsize))
                            {
                                ConvertedItemsCounter++;
                            }
                            else
                            {
                                Logger.LogInformation($"fsize {fsize.Term.LowerName} already set for post {post.ID}.");
                            }

                            
                        }
                        WriteToResultFile($"postId: {post.ID} {size.Term.LowerName} => {string.Join(",", fsizeNames)}");
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
