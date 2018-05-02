using Converter.DAL;
using Microsoft.Extensions.Configuration;
using System;

namespace Converter
{
    class Program
    {
        static IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
              .AddJsonFile("appsettings.json")
              //.AddJsonFile("appsettings.Development.json")
              //.AddEnvironmentVariables()
              .Build();
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var config = GetConfiguration();
            using (var dao = new Dao(config["ConnectionString"]))
            {
                var posts = dao.GetPosts();

            }
        }

       
    }
}
