using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Converter.Settings;

namespace Converter.Color
{

    class ColorConvertStrategy : BaseConvertStrategy<ColorConverterSettings>
    {
        public ColorConvertStrategy(Dao dao, ILogger<ColorConvertStrategy> logger, ColorConverterSettings settings) : base(dao, logger, settings)
        {
           
        }

        public void Execute()
        {
            ResetResultFiles();
            
                if (Settings.DeleteAllFColours)
                {
                    Dao.DeleteAllFColours();
                    Logger.LogInformation("All fcolours and their relationships were deleted");
                }

                UpdateFColours(Dao);

                var fcolours = Dao.GetFColours();
                var colorConverter = CreateColorConverter(fcolours, Settings);
                var posts = Dao.GetPosts();

                foreach (var p in posts)
                {
                    ConverPost(p, colorConverter);
                }

                if (Settings.SaveResult)
                {
                    //сохним новые fcolours
                    Dao.SaveChanges();

                    //проставим количество
                    var taxonomyWithCounts = Dao.GetTaxonomyCount();

                    foreach (var fcolor in fcolours)
                    {
                        fcolor.count = taxonomyWithCounts[fcolor.term_taxonomy_id];
                    }

                    Dao.SaveChanges();
                }

            
            WriteResults();
        }

        void UpdateFColours(Dao dao)
        {
            if (Settings.FColours.Any())
            {
                var existedFColours = dao
                    .GetFColours(true);
                var existedFColoursNames = existedFColours.Select(x => x.Term.LowerName).ToArray();

                var forDelete = existedFColours
                    .Where(x => !Settings.FColours.Contains(x.Term.LowerName))
                    .ToArray();

                var forAdd = Settings
                    .FColours
                    .Where(x => !existedFColoursNames.Contains(x))
                    .ToArray();

                dao.DeleteFColours(forDelete);
                dao.CreateFColours(forAdd);
                dao.SaveChanges();

                if (forDelete.Any())
                {
                    Logger.LogInformation($"fcolours were deleted: {string.Join(",", forDelete.Select(x => x.Term.LowerName))}");
                }
                if (forAdd.Any())
                {
                    Logger.LogInformation($"fcolours were added: {string.Join(",", forAdd)}");
                }
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
                        UnknownItemsCounter++;
                    }
                    else
                    {
                        foreach (var fcolor in fcolours)
                        {
                            if (!post.SetFColour(fcolor))
                            {
                                Logger.LogInformation($"fcolor {fcolor.Term.LowerName} already set for post {post.ID}.");
                            }
                            else
                            {
                                //запись дублируется в логе, вынести из цикла
                                var fcoloursNames = fcolours.Select(x => x.Term.LowerName);
                                WriteToResultFile($"postId: {post.ID} {color.Term.LowerName} => {string.Join(",", fcoloursNames)}");
                                ConvertedItemsCounter++;
                            }

                        }

                    }
                }
            }
            else
            {
                Logger.LogInformation($"Post {post.ID} has no colours");
            }
        }
    }
}
