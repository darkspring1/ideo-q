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
        const string CMD_SELECT_TERM_TAXONOMY_COUNT = @"

SELECT tr.term_taxonomy_id term_taxonomy_id, COUNT(*) count FROM wp_term_relationships tr
	JOIN wp_term_taxonomy tt
		ON tt.term_taxonomy_id = tr.term_taxonomy_id
		
	WHERE tt.taxonomy = '{0}'
	GROUP BY (tr.term_taxonomy_id);

";

        const string CMD_DELETE_ALL_FCOLOURS = @"
START TRANSACTION;

DELETE tr
FROM wp_term_relationships tr
JOIN wp_term_taxonomy tt
	ON tr.term_taxonomy_id = tt.term_taxonomy_id
WHERE tt.taxonomy = 'pa_fcolor';

DELETE t
FROM wp_terms t
JOIN wp_term_taxonomy tt
	ON t.term_id = tt.term_id
WHERE tt.taxonomy = 'pa_fcolor';

DELETE tt
FROM wp_term_taxonomy tt
WHERE tt.taxonomy = 'pa_fcolor';

COMMIT;";


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

        IDictionary<long, long> GetTaxonomyCount(string taxonomy)
        {
            return DataContext
                .Set<TaxonomyWithCount>()
                .FromSql(string.Format(CMD_SELECT_TERM_TAXONOMY_COUNT, taxonomy))
                .ToDictionary(x => x.term_taxonomy_id, x => x.count);
        }

        public IDictionary<long, long> GetFColorTaxonomyCount()
        {
            return GetTaxonomyCount(Taxonomy.PA_FCOLOR);
        }

        TermTaxonomy[] GetTermTaxonomies(string taxonomy, bool asNoTracking = false)
        {
            var q = DataContext.Set<TermTaxonomy>()
               .Where(x => x.taxonomy == taxonomy)
               .Include(x => x.Term)
               .GroupBy(x => x.Term.name)
               .Select(x => x.First());

            if (asNoTracking)
            {
                q = q.AsNoTracking();
            }

            return q.ToArray();
        }

        public TermTaxonomy[] GetFColours(bool asNoTracking = false)
        {
            return GetTermTaxonomies(Taxonomy.PA_FCOLOR, asNoTracking);
        }

        public TermTaxonomy[] GetFSizes(bool asNoTracking = false)
        {
            return GetTermTaxonomies(Taxonomy.PA_FSIZE, asNoTracking);
        }

        /// <summary>
        /// Удалить переданные TermTaxonomy из таблиц: wp_terms, wp_term_taxonomy, wp_termrelationship
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
        /// Удалить все TermTaxonomy c taxonomy='pa_fcolor' из таблиц: wp_terms, wp_term_taxonomy, wp_termrelationship
        /// </summary>
        public void DeleteAllFColours()
        {
            DataContext.Database.ExecuteSqlCommand(CMD_DELETE_ALL_FCOLOURS);
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

        public TermTaxonomy CreateFSize(string fsize)
        {
            var entitiy = TermTaxonomy.CreateFSize(fsize);
            _dataContext.AddRange(entitiy);
            return entitiy;
        }

        public void SaveChanges()
        {
            _dataContext.SaveChanges();
        }


    }
}
