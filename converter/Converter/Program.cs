using Converter.Color;
using Converter.DAL;
using Converter.Settings;
using Converter.Size;
using Microsoft.Extensions.Caching.Memory;
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
                using (var dao = new Dao(settings.TablePrefix, settings.ConnectionString))
                {
                    dao.InstallCustomAttributes();

                    if (settings.ColorConverterSettings.IsEnabled)
                    {
                        var colorStrategy = new ColorConvertStrategy(dao, loggerFactory.CreateLogger<ColorConvertStrategy>(), settings.ColorConverterSettings);
                        colorStrategy.Execute();
                    }
                    if (settings.SizeConverterSettings.IsEnabled)
                    {
                        using (var mc = new MemoryCache(new MemoryCacheOptions()))
                        {
                            var sizeStrategy = new SizeConvertStrategy(dao, loggerFactory.CreateLogger<SizeConvertStrategy>(), settings.SizeConverterSettings, mc);
                            sizeStrategy.Execute();
                        }
                    }
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
