using System;
using System.Collections.Generic;
using System.Linq;
using Converter.DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace Converter.DAL
{
    class Dao : IDisposable
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


        private DataContext _dataContext;
        private readonly string _connectionString;

        private DataContext DataContext
        {
            get
            {
                if (_dataContext == null)
                {
                    _dataContext = new DataContext(_connectionString);
                }

                return _dataContext;
            }
        }
        
        public Dao(string connectionString)
        {
            this._connectionString = connectionString;
        }

        public void Dispose()
        {
            if (_dataContext != null)
            {
                _dataContext.Dispose();
            }
        }

        public IDictionary<long, Post> GetPosts()
        {
            var posts = DataContext.Set<Post>()
                .Where(x => x.post_type == "product")
                .Include(x => x.TermRelationships)
                .ToArray();

            return posts.ToDictionary(p => p.ID);
        }


    }
}
