using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Converter
{
    class ColorConverStrategy
    {
        private class NotConvertedColorInfo
        {
            public long PostId { get; set; }
            public string TermName { get; set; }
            public long TermId { get; set; }
        }

        private readonly ILogger<ColorConverStrategy> _logger;
        private readonly Settings _settings;

        private readonly List<NotConvertedColorInfo> _notConvertedColours;
        private long _convertedColoursCounter = 0;

        public ColorConverStrategy(ILogger<ColorConverStrategy> logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
            _notConvertedColours = new List<NotConvertedColorInfo>();
        }

        public void Execute()
        {
            using (var dao = new Dao(_settings.ConnectionString))
            {
                UpdateFColours(dao);
                var colorConverter = CreateColorConverter(dao, _settings);
                var posts = dao.GetPosts();

                foreach (var p in posts)
                {
                    ConverPost(p, colorConverter);
                }
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
            }
            
        }

        void WriteResults()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_settings.ResultFile));
            Directory.CreateDirectory(Path.GetDirectoryName(_settings.UnknownColoursFile));

            File.Delete(_settings.ResultFile);
            File.Delete(_settings.UnknownColoursFile);

            using (var resultFile = File.CreateText(_settings.ResultFile))
            {
                resultFile.WriteLine(DateTime.Now.ToString());
                resultFile.WriteLine($"{_convertedColoursCounter} were converted.");

                if (_notConvertedColours.Any())
                {
                    resultFile.WriteLine($"{_notConvertedColours.Count()} were not converted. See {_settings.UnknownColoursFile} .");
                    using (var uknownColoursFile = File.CreateText(_settings.UnknownColoursFile))
                    {
                        var line = string.Join(System.Environment.NewLine, _notConvertedColours.Select(x => $"PostId:{x.PostId}, TermId:{x.TermId}, TermId:{x.TermName}"));
                        uknownColoursFile.WriteLine(line);
                    }
                }
            }

        }

        ColorConverter CreateColorConverter(Dao dao, Settings settings)
        {
            var mapping = new Dictionary<string, List<TermTaxonomy>>();
            var fcolours = dao.GetFilterableColours();

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
                    if (fcolours == null)
                    {
                        _notConvertedColours.Add(new NotConvertedColorInfo
                        {
                            PostId = post.ID,
                            TermId = color.term_id,
                            TermName = color.Term.LowerName
                        });
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
                                _logger.LogTrace($"{color} => {string.Join(",", fcolours)}");
                                _convertedColoursCounter++;
                            }

                        }
                        
                    }
                }
            }
        }
    }
}
