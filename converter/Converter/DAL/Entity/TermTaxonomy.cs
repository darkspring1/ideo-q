using Converter.DAL.Constants;

namespace Converter.DAL.Entity
{
    class TermTaxonomy
    {

        private TermTaxonomy()
        {

        }

        public long term_taxonomy_id { get; set; }
        public long term_id { get; set; }
        public string taxonomy { get; set; }
        public string description { get; set; }
        public long count { get; set; }

        public Term Term { get; set; }


        public static TermTaxonomy CreateFColour(string fcolourName)
        {
            var termTaxonomy = new TermTaxonomy
            {
                taxonomy = Taxonomy.PA_FCOLOR,
                description = $"color for filter '{fcolourName}'",
                Term = new Term
                {
                    slug = fcolourName,
                    name = fcolourName
                }
            };

            return termTaxonomy;
        }

        public static TermTaxonomy CreateFSize(string fsizeName)
        {
            var termTaxonomy = new TermTaxonomy
            {
                taxonomy = Taxonomy.PA_FSIZE,
                description = $"size for filter '{fsizeName}'",
                Term = new Term
                {
                    slug = fsizeName,
                    name = fsizeName
                }
            };

            return termTaxonomy;
        }
    }
}
