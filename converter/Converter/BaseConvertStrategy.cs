using Converter.DAL;
using Microsoft.Extensions.Logging;
using System.IO;
using Converter.Settings;

namespace Converter
{
    class BaseConvertStrategy<T>
        where T : BaseConverterSettings
    {
        protected Dao Dao { get; }
        protected  ILogger Logger { get; }
        protected T Settings { get; }

        protected long ConvertedItemsCounter = 0;
        protected long UnknownItemsCounter = 0;

        public BaseConvertStrategy(Dao dao, ILogger logger, T settings)
        {
            Dao = dao;
            Logger = logger;
            Settings = settings;
        }

        /// <summary>
        /// пересоздадим файлы с результатами работы
        /// </summary>
        protected void ResetResultFiles()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.ResultFile));
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.UnknownFile));

            File.Delete(Settings.ResultFile);
            File.Delete(Settings.UnknownFile);

            using (File.CreateText(Settings.ResultFile)) { }
            using (File.CreateText(Settings.UnknownFile)) { }
        }

        protected void WriteToResultFile(params string[] lines)
        {
            File.AppendAllLines(Settings.ResultFile, lines);
        }

        protected void WriteToUnknownColoursFile(params string[] lines)
        {
            File.AppendAllLines(Settings.UnknownFile, lines);
        }


        protected void WriteResults()
        {
            WriteToResultFile($"{ConvertedItemsCounter} units were converted.");
            if (Settings.SaveResult)
            {
                WriteToResultFile("All the results were saved to database");
            }
            else
            {
                WriteToResultFile("The results were not saved to database. If you want to save it set the 'SaveResult' flag 'true' in appsettings.json");
            }

            if (UnknownItemsCounter > 0)
            {
                WriteToResultFile($"{UnknownItemsCounter} colours were not converted. See {Settings.UnknownFile} .");
            }
            else
            {
                File.Delete(Settings.UnknownFile);
            }

        }
    }
}
