using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
                var colorConverter = CreateColorConverter(dao, _settings);
                var posts = dao.GetPosts();

                foreach (var p in posts)
                {
                    ConverPost(p, colorConverter);
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
                    var termTaxonomy = fcolours.FirstOrDefault(x => x.Term.name.ToLower() == colorName);
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
                            TermName = color.Term.name
                        });
                    }
                    else
                    {
                        foreach (var fcolor in fcolours)
                        {
                            if (!post.SetFColour(fcolor))
                            {
                                _logger.LogInformation($"fcolor {fcolor.Term.name} already set for post {post.ID}.");
                            }
                        }
                        
                    }
                }
            }
        }
    }
}
