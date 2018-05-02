namespace Converter.DAL.Entity
{
    class TermTaxonomy
    {
        public long term_taxonomy_id { get; set; }
        public long term_id { get; set; }
        public string taxonomy { get; set; }
        public string description { get; set; }
        public long count { get; set; }

        public Term Term { get; set; }
    }
}
