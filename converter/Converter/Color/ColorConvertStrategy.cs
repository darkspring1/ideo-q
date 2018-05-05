using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Converter.Settings;

namespace Converter.Color
{
    class ColorConvertStrategy
    {
        private readonly Dao _dao;
        private readonly ILogger<ColorConvertStrategy> _logger;
        private readonly ColorConverterSettings _settings;

        private long _convertedColoursCounter = 0;
        private long _unknownColoursCounter = 0;

        public ColorConvertStrategy(Dao dao, ILogger<ColorConvertStrategy> logger, ColorConverterSettings settings)
        {
            _dao = dao;
            _logger = logger;
            _settings = settings;
        }

        public void Execute()
        {
            ResetResultFiles();
            
                if (_settings.DeleteAllFColours)
                {
                    _dao.DeleteAllFColours();
                    _logger.LogInformation("All fcolours and their relationships were deleted");
                }

                UpdateFColours(_dao);

                var fcolours = _dao.GetFilterableColours();
                var colorConverter = CreateColorConverter(fcolours, _settings);
                var posts = _dao.GetPosts();

                foreach (var p in posts)
                {
                    ConverPost(p, colorConverter);
                }

                if (_settings.SaveResult)
                {
                    //сохним новые fcolours
                    _dao.SaveChanges();

                    //проставим количество
                    var taxonomyWithCounts = _dao.GetTaxonomyCount();

                    foreach (var fcolor in fcolours)
                    {
                        fcolor.count = taxonomyWithCounts[fcolor.term_taxonomy_id];
                    }

                    _dao.SaveChanges();
                }

            
            WriteResults();
        }

        
        void UpdateFColours(Dao dao)
        {
            if (_settings.FColours.Any())
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

        /// <summary>
        /// пересоздадим файлы с результатами работы
        /// </summary>
        void ResetResultFiles()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settings.ResultFile));
            Directory.CreateDirectory(Path.GetDirectoryName(_settings.UnknownColoursFile));

            File.Delete(_settings.ResultFile);
            File.Delete(_settings.UnknownColoursFile);

            using (File.CreateText(_settings.ResultFile)) { }
            using (File.CreateText(_settings.UnknownColoursFile)) { }
        }


        void WriteToResultFile(params string[] lines)
        {
            File.AppendAllLines(_settings.ResultFile, lines);
        }

        void WriteToUnknownColoursFile(params string[] lines)
        {
            File.AppendAllLines(_settings.UnknownColoursFile, lines);
        }


        void WriteResults()
        {
            WriteToResultFile($"{_convertedColoursCounter} colours were converted.");
            if (_settings.SaveResult)
            {
                WriteToResultFile("All the results were saved to database");
            }
            else
            {
                WriteToResultFile("The results were not saved to database. If you want to save it set the 'SaveResult' flag 'true' in appsettings.json");
            }

            if (_unknownColoursCounter > 0)
            {
                WriteToResultFile($"{_unknownColoursCounter} colours were not converted. See {_settings.UnknownColoursFile} .");
            }
            else
            {
                File.Delete(_settings.UnknownColoursFile);
            }

        }

        ColorConverter CreateColorConverter(TermTaxonomy[] fcolours, ColorConverterSettings settings)
        {
            var mapping = new Dictionary<string, List<TermTaxonomy>>();
            
            foreach (var mappingItem in settings.ColorMapping)
            {
                var key = mappingItem.Key;
                var value = new List<TermTaxonomy>();
                foreach (var colorName in mappingItem.Value)
                {
                    var termTaxonomy = fcolours.FirstOrDefault(x => x.Term.LowerName == colorName);
                    if (termTaxonomy != null)
                    {
                        value.Add(termTaxonomy);
                    }
                }
                mapping.Add(key, value);
            }

            return new ColorConverter(fcolours, mapping);
        }

        void ConverPost(Post post, ColorConverter converter)
        {
            if (post.Colours.Any())
            {
                foreach (var color in post.Colours)
                {
                    var fcolours = converter.ConvertToFilterable(color);
                    if (fcolours == null || !fcolours.Any())
                    {
                        WriteToUnknownColoursFile($"PostId:{post.ID}, TermId:{color.term_id}, TermName:{color.Term.LowerName}");
                        _unknownColoursCounter++;
                    }
                    else
                    {
                        foreach (var fcolor in fcolours)
                        {
                            if (!post.SetFColour(fcolor))
                            {
                                _logger.LogInformation($"fcolor {fcolor.Term.LowerName} already set for post {post.ID}.");
                            }
                            else
                            {
                                var fcoloursNames = fcolours.Select(x => x.Term.LowerName);
                                WriteToResultFile($"postId: {post.ID} {color.Term.LowerName} => {string.Join(",", fcoloursNames)}");
                                _convertedColoursCounter++;
                            }

                        }

                    }
                }
            }
            else
            {
                _logger.LogInformation($"Post {post.ID} has no colours");
            }
        }
    }
}
