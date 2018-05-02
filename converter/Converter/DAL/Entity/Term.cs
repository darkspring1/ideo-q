namespace Converter.DAL.Entity
{
    class Term
    {
        public long term_id { get; set; }
        public string name { get; set; }

        public string LowerName => name.ToLower();

        public string slug { get; set; }
    }
}
