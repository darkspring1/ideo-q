using Converter.DAL;
using Converter.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;

namespace Converter
{
    class Program
    {
        static MainSettings CreateSettings()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              .Build();

            return new MainSettings(config);
        }

        static ILoggerFactory CreateLoggerFactory()
        {
            ILoggerFactory loggerFactory = new LoggerFactory().AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
            NLog.LogManager.LoadConfiguration("nlog.config");
            return loggerFactory;
        }

        static void Main(string[] args)
        {
            var loggerFactory = CreateLoggerFactory();
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("STARTED");
            try
            {
                var settings = CreateSettings();
                using (var dao = new Dao(settings.ConnectionString))
                {
                    var strategy = new ColorConverStrategy(dao, loggerFactory.CreateLogger<ColorConverStrategy>(), settings.ColorConverterSettings);
                    strategy.Execute();
                }
                logger.LogInformation("FINISHED");
                Console.WriteLine("Press any key.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                logger.LogError(e.Message, e);
            }
        }

    }
}
