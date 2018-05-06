using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Converter.Settings;

namespace Converter.Size
{
    class SizeConvertStrategy
    {
        private readonly Dao _dao;
        private readonly ILogger<SizeConvertStrategy> _logger;
        private readonly SizeConverterSettings _settings;

        private long _convertedColoursCounter = 0;
        private long _unknownColoursCounter = 0;

        public SizeConvertStrategy(Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings)
        {
            _dao = dao;
            _logger = logger;
            _settings = settings;
        }

        public void Execute()
        {
            var posts = _dao.GetPosts();

            foreach (var p in posts)
            {
                ConverPost(p, colorConverter);
            }
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
