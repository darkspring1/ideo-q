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
            if (directMapping.TryGetValue(color.Term.name, out result))
            {
                return result;
            }

            foreach (var fcolour in _filterableColours)
            {
                if (color.Term.name.Contains(fcolour.Term.name))
                {
                    result.Add(fcolour);
                }
            }

            return result;
        }


    }
}
