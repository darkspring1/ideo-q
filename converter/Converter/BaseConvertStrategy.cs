using Converter.DAL;
using Microsoft.Extensions.Logging;
using System.IO;
using Converter.Settings;
using Converter.DAL.Entity;

namespace Converter
{
    abstract class BaseConvertStrategy<T>
        where T : BaseConverterSettings
    {
        protected Dao Dao { get; }
        protected  ILogger Logger { get; }
        protected T Settings { get; }

        private long ConvertedItemsCounter = 0;
        private long UnknownItemsCounter = 0;
        private readonly string _fAttributeTaxonomy;

        public BaseConvertStrategy(string fAttributeTaxonomy, Dao dao, ILogger logger, T settings)
        {
            _fAttributeTaxonomy = fAttributeTaxonomy;
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
            ConvertedItemsCounter++;
        }

        protected void WriteToUnknownFile(params string[] lines)
        {
            File.AppendAllLines(Settings.UnknownFile, lines);
            UnknownItemsCounter++;
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


        protected abstract void BeforeExecute();

        protected abstract void AfterSave();

        protected abstract void ConverPost(Post post);

        public void Execute()
        {
            ResetResultFiles();

            if (Settings.DeleteFAttributes)
            {
                Dao.DeleteAllFAttributes(_fAttributeTaxonomy);
                Logger.LogInformation($"All fAttributes('{_fAttributeTaxonomy}') and their relationships were deleted");
            }

            BeforeExecute();

            var posts = Dao.GetPosts();

            foreach (var p in posts)
            {
                ConverPost(p);
            }

            if (Settings.SaveResult)
            {
                Logger.LogInformation($"Save results...");
                //сохним новые fcolours
                Dao.SaveChanges();

                AfterSave();
            }


            WriteResults();
        }
    }
}
