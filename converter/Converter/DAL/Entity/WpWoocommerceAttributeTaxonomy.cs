namespace Converter.DAL.Entity
{
    public class WpWoocommerceAttributeTaxonomy
    {
        public long attribute_id { get; set; }
        public string attribute_name { get; set; }
        public string attribute_label { get; set; }
        public string attribute_type { get; set; }
        public string attribute_orderby { get; set; }
        public int attribute_public { get; set; }


        public static WpWoocommerceAttributeTaxonomy Create(string attributeName)
        {
            return new WpWoocommerceAttributeTaxonomy
            {
                attribute_name = attributeName,
                attribute_label = attributeName,
                attribute_type = "select",
                attribute_public = 0,
                attribute_orderby = "menu_order"
            };
        }
    }
}
