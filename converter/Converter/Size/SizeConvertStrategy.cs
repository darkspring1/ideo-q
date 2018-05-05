using Converter.DAL;
using Converter.DAL.Entity;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Converter.Settings;

namespace Converter.Size
{
    class SizeConvertStrategy
    {
        private readonly Dao _dao;
        private readonly ILogger<SizeConvertStrategy> _logger;
        private readonly SizeConverterSettings _settings;

        private long _convertedColoursCounter = 0;
        private long _unknownColoursCounter = 0;

        public SizeConvertStrategy(Dao dao, ILogger<SizeConvertStrategy> logger, SizeConverterSettings settings)
        {
            _dao = dao;
            _logger = logger;
            _settings = settings;
        }

        public void Execute()
        {
        
        }
        
        
    }
}
