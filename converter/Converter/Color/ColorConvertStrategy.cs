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
        ColorConverter _converter;
        TermTaxonomy[] _fcolours;

        public ColorConvertStrategy(Dao dao, ILogger<ColorConvertStrategy> logger, ColorConverterSettings settings) : base(dao, logger, settings)
        {
           
        }

        protected override void BeforeExecute()
        {
            if (Settings.DeleteAllFColours)
            {
                Dao.DeleteAllFColours();
                Logger.LogInformation("All fcolours and their relationships were deleted");
            }

            UpdateFColours(Dao);

            _fcolours = Dao.GetFColours();
            _converter = CreateColorConverter(_fcolours, Settings);
        }

        protected override void AfterSave()
        {
            //проставим количество
            var taxonomyWithCounts = Dao.GetFColorTaxonomyCount();

            foreach (var fcolor in _fcolours)
            {
                fcolor.count = taxonomyWithCounts[fcolor.term_taxonomy_id];
            }

            Dao.SaveChanges();
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


        protected override void ConverPost(Post post)
        {
            if (post.Colours.Any())
            {
                foreach (var color in post.Colours)
                {
                    var fcolours = _converter.ConvertToFilterable(color);
                    if (fcolours == null || !fcolours.Any())
                    {
                        WriteToUnknownFile($"PostId:{post.ID}, TermId:{color.term_id}, TermName:{color.Term.LowerName}");
                    }
                    else
                    {
                        var setFColorNames = "";
                        foreach (var fcolor in fcolours)
                        {
                            if (post.SetFColour(fcolor))
                            {
                                setFColorNames += $"{fcolor.Term.LowerName},";
                            }
                            else
                            {
                                Logger.LogInformation($"fcolor {fcolor.Term.LowerName} already set for post {post.ID}.");
                            }
                        }

                        if (setFColorNames != "")
                        {
                            WriteToResultFile($"postId: {post.ID} {color.Term.LowerName} => {setFColorNames.TrimEnd(',')}");
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
