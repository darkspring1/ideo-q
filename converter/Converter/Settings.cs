using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Converter
{
    class Settings
    {
        private readonly IConfiguration _config;

        public Settings(IConfiguration config)
        {
            _config = config;
        }


        public string ConnectionString
        {
            get
            {
                return _config["ConnectionString"];
            }
        }

        public IDictionary<string, List<string>> ColorMapping
        {
            get
            {
                var sections = _config.GetSection("ColorMapping");
                return sections
                    .AsEnumerable(true)
                    .ToDictionary(
                        kvp => kvp.Key.ToLower(),
                        kvp => kvp
                            .Value
                            .Split(",")
                            .Select(x => x.TrimStart().TrimEnd().ToLower())
                            .ToList() );
            }
        }
    }
}
