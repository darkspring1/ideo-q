using Converter.DAL;
using Microsoft.Extensions.Logging;
using Converter.Settings;
using System.Linq;
using Converter.DAL.Entity;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Converter.Size
{
    class SizeConvertStrategy
    {
        const string FSIZE_CACHE_KEY_TEMPLATE = "fsize_{0}";
    private readonly Dao _dao;
        private readonly ILogger<SizeConvertStrategy> _logger;
        private readonly SizeConverterSettings _settings;
        private readonly IMemoryCache _cache;
        private long _convertedSizesCounter = 0;
        private long _unknownColoursCounter = 0;

        public SizeConvertStrategy(Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings, IMemoryCache cache)
        {
            _dao = dao;
            _logger = logger;
            _settings = settings;
            _cache = cache;
        }

        public void Execute()
        {
            //UpdateFColours();
            LoadExistedFSizesToCache();
            var posts = _dao.GetPosts();

            foreach (var p in posts)
            {
                ConverPost(p/*, colorConverter*/);
            }
            
        }


        string[] GetFSizesFromSettings()
        {
            return _settings
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
            var fsizes = _dao.GetFSizes(true);

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

                    var binding = _settings.SizeChartBindings.FirstOrDefault(x => x.Categories.Contains(categoryName));
                    if (binding == null)
                    {
                        return null;
                    }
                    return _settings.SizeCharts.First(sc => sc.Name == binding.SizeChartName);
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
                return _dao.CreateFSize(fsizeName);
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
                        _logger.LogInformation($"Post {post.ID} has no size chart");
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
                                _convertedSizesCounter++;
                            }
                            else
                            {
                                _logger.LogInformation($"fsize {fsize.Term.LowerName} already set for post {post.ID}.");
                            }

                            
                        }
                        //WriteToResultFile($"postId: {post.ID} {size.Term.LowerName} => {string.Join(",", fsizeNames)}");



                    }
                    
                }
                else
                {
                    _logger.LogInformation($"Post {post.ID} has no categories");
                }
            }
            else
            {
                _logger.LogInformation($"Post {post.ID} has no sizes");
            }
        }


    }
}
