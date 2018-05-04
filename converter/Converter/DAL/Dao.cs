using System;
using System.Collections.Generic;
using System.Linq;
using Converter.DAL.Constants;
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

        public Post[] GetPosts()
        {
            var posts = DataContext.Set<Post>()
                .Where(x => x.post_type == "product")
                .Include(x => x.TermRelationships)
                .ThenInclude(x => x.TermTaxonomy)
                .ThenInclude(x => x.Term)
                .ToArray();

            return posts;
        }

        public TermTaxonomy[] GetFilterableColours(bool asNoTracking = false)
        {
            var q = DataContext.Set<TermTaxonomy>()
                .Where(x => x.taxonomy == Taxonomy.PA_FCOLOR)
                .Include(x => x.Term)
                .GroupBy(x => x.Term.name)
                .Select(x => x.First());

            if (asNoTracking)
            {
                q = q.AsNoTracking();
            }

            return q.ToArray();
        }

        /// <summary>
        /// удалит fcolours  вместе со сылающимися на них TermRelationship
        /// </summary>
        /// <param name="termTaxonomiesForRemove"></param>
        public void DeleteFColours(TermTaxonomy[] termTaxonomiesForRemove)
        {
            if (termTaxonomiesForRemove.Any())
            {
                var taxonomyIds = string.Join(",", termTaxonomiesForRemove.Select(x => x.term_taxonomy_id));
                var termIds = string.Join(",", termTaxonomiesForRemove.Select(x => x.term_id));

                var deleteTermRelationshipCmd = $"DELETE FROM {Tables.TermRelationships} WHERE term_taxonomy_id IN ({taxonomyIds})";
                var deleteTermTaxonomyCmd = $"DELETE FROM {Tables.TermTaxonomy} WHERE term_taxonomy_id IN ({taxonomyIds})";
                var deleteTermCmd = $"DELETE FROM {Tables.Terms} WHERE term_id IN ({termIds})";
                _dataContext.Database.ExecuteSqlCommand(deleteTermRelationshipCmd);
                _dataContext.Database.ExecuteSqlCommand(deleteTermTaxonomyCmd);
                _dataContext.Database.ExecuteSqlCommand(deleteTermCmd);
            }
        }

        /// <summary>
        /// Создаст FColours в БД
        /// </summary>
        /// <param name="fcolours"></param>
        public void CreateFColours(string[] fcolours)
        {
            var entities = fcolours.Select(x => TermTaxonomy.CreateFColour(x)).ToArray();
            _dataContext.AddRange(entities);
        }

        public void SaveChanges()
        {
            _dataContext.SaveChanges();
        }


    }
}
