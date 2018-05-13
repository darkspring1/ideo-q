using Converter.DAL.Constants;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Converter.Settings
{

    public class BraSizeChart : List<IDictionary<string, string>>, ISizeChart
    {
       

        void AddItems(List<Dictionary<string, string>> braSizeDictionaries, Dictionary<string, string>  cupSzDictionary)
        {
            Func<string, Dictionary<string, string>, KeyValuePair<string, string>> createItem = (prefix, braSizes) =>
            {
                return new KeyValuePair<string, string>($"{prefix}{braSizes[prefix]}{cupSzDictionary[prefix]}", $"{prefix} {braSizes[prefix]} {cupSzDictionary[prefix]}");
            };

            foreach (var braSizes in braSizeDictionaries)
            {
                var dict = new Dictionary<string, string>(new[] {
                    createItem(SizePrefix.US, braSizes),
                    createItem(SizePrefix.UK, braSizes),
                    createItem(SizePrefix.EUR, braSizes)
                });

                Add(dict);
            }
        }

        public BraSizeChart(IConfigurationSection braSizesSection, IConfigurationSection cupSizesSection)
        {
            var braSzsSections = braSizesSection.GetChildren();
            var cupSzsSections = cupSizesSection.GetChildren();

            var braSizeDictionaries = new List<Dictionary<string, string>>();
            foreach (var braSzs in braSzsSections)
            {
                braSizeDictionaries.Add( braSzs.GetChildren().ToDictionary(x => x.Value.WithoutValue(), x => x.Value.WithoutPrefix()) );
            }

            foreach (var cupSzs in cupSzsSections)
            {
                var cupSzDictionary = cupSzs.GetChildren().ToDictionary(x => x.Value.WithoutValue(), x => x.Value.WithoutPrefix());
                AddItems(braSizeDictionaries, cupSzDictionary);
            }
        }

        public string Name { get; }
    }
}
