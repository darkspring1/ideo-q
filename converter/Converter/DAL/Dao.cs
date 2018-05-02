using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using Dapper;
using System.Linq;

namespace Converter.DAL
{
    class Dao
    {
        const string CMD_POSTS_WITH_COLOURS_AND_FCOLOURS = @"SELECT
p.ID postId,
p.post_title postTitle,
t.term_id termId,
t.name,
tt.taxonomy taxonomy
FROM wp_posts p
JOIN wp_term_relationships tr ON tr.object_id = p.ID
JOIN wp_term_taxonomy tt on tt.term_taxonomy_id = tr.term_taxonomy_id
JOIN wp_terms t ON t.term_id = tt.term_id
WHERE post_type = 'product'
AND (taxonomy = 'pa_color' OR taxonomy = 'f_color')
ORDER BY p.ID";
        //private readonly string connectionString;

        private readonly Lazy<MySqlConnection> _connection;
        

        
        public Dao(string connectionString)
        {
            //this.connectionString = connectionString;

            _connection = new Lazy<MySqlConnection>(() =>
            {
                var conn = new MySqlConnection(connectionString);
                conn.Open();
                return conn;
            });
        }

        public void Dispose()
        {
            if (_connection.IsValueCreated)
            {
                _connection.Value.Dispose();
            }
        }

        


        public IDictionary<long, PostWithColorAndFColor> GetPosts()
        {
            var posts = _connection.Value.Query<PostWithColorAndFColor>(CMD_POSTS_WITH_COLOURS_AND_FCOLOURS);

            return posts.ToDictionary(p => p.PostId);
        }


    }
}
