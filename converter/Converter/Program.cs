using MySql.Data.MySqlClient;
using System;

namespace Converter
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            using (var conn = new MySqlConnection("server=localhost;port=3306;database=test-ideo-q;user=root;SslMode=none;"))
            {
                conn.Open();
                MySqlCommand cmd = new MySqlCommand("select * from wp_posts", conn);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        /*
                        list.Add(new Album()
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            ArtistName = reader["ArtistName"].ToString(),
                            Price = Convert.ToInt32(reader["Price"]),
                            Genre = reader["genre"].ToString()
                        });*/
                    }
                }
            }
        }

       
    }
}
