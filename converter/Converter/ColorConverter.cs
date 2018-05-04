using Converter.DAL.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Converter
{
    class ColorConverter
    {
        public ColorConverter(TermTaxonomy[] filterableColours, IDictionary<string, List<TermTaxonomy>> directMapping)
        {
            _filterableColours = filterableColours;
            this.directMapping = directMapping;
        }

        TermTaxonomy[] _filterableColours;
        private readonly IDictionary<string, List<TermTaxonomy>> directMapping;

        public List<TermTaxonomy> ConvertToFilterable(TermTaxonomy color)
        {
            List<TermTaxonomy> result;
            if (directMapping.TryGetValue(color.Term.LowerName, out result))
            {
                return result;
            }

            result = new List<TermTaxonomy>();
            foreach (var fcolour in _filterableColours)
            {
                if (color.Term.LowerName.Contains(fcolour.Term.LowerName))
                {
                    result.Add(fcolour);
                }
            }

            return result;
        }


    }
}
