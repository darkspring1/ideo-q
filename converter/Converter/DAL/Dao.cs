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

SELECT tr.term_taxonomy_id term_taxonomy_id, COUNT(*) count FROM {0}term_relationships tr
	JOIN {0}term_taxonomy tt
		ON tt.term_taxonomy_id = tr.term_taxonomy_id
		
	WHERE tt.taxonomy = '{1}'
	GROUP BY (tr.term_taxonomy_id);
";

        const string CMD_DELETE_ALL_FATTRS = @"
START TRANSACTION;

DELETE tr
FROM {0}term_relationships tr
JOIN {0}term_taxonomy tt
	ON tr.term_taxonomy_id = tt.term_taxonomy_id
WHERE tt.taxonomy = '{1}';

DELETE t
FROM {0}terms t
JOIN {0}term_taxonomy tt
	ON t.term_id = tt.term_id
WHERE tt.taxonomy = '{1}';

DELETE tt
FROM {0}term_taxonomy tt
WHERE tt.taxonomy = '{1}';

COMMIT;";


        private DataContext _dataContext;
        private readonly string _connectionString;

        private DataContext DataContext
        {
            get
            {
                if (_dataContext == null)
                {
                    _dataContext = new DataContext(_prefix, _connectionString);
                }

                return _dataContext;
            }
        }

        readonly string _prefix;

        public Dao(string prefix, string connectionString)
        {
            _prefix = prefix;
            _connectionString = connectionString;
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
                .FromSql(string.Format(CMD_SELECT_TERM_TAXONOMY_COUNT, _prefix, taxonomy))
                .ToDictionary(x => x.term_taxonomy_id, x => x.count);
        }

        public IDictionary<long, long> GetFColorTaxonomyCount()
        {
            return GetTaxonomyCount(Taxonomy.PA_FCOLOR);
        }

        public IDictionary<long, long> GetFSizeTaxonomyCount()
        {
            return GetTaxonomyCount(Taxonomy.PA_FSIZE);
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

                var deleteTermRelationshipCmd = $"DELETE FROM {_prefix}{Tables.TermRelationships} WHERE term_taxonomy_id IN ({taxonomyIds})";
                var deleteTermTaxonomyCmd = $"DELETE FROM {_prefix}{Tables.TermTaxonomy} WHERE term_taxonomy_id IN ({taxonomyIds})";
                var deleteTermCmd = $"DELETE FROM {_prefix}{Tables.Terms} WHERE term_id IN ({termIds})";
                _dataContext.Database.ExecuteSqlCommand(deleteTermRelationshipCmd);
                _dataContext.Database.ExecuteSqlCommand(deleteTermTaxonomyCmd);
                _dataContext.Database.ExecuteSqlCommand(deleteTermCmd);
            }
        }

        /// <summary>
        /// Удалить все TermTaxonomy c taxonomy='pa_fcolor' или 'pa_fsize' из таблиц: wp_terms, wp_term_taxonomy, wp_termrelationship
        /// </summary>
        public void DeleteAllFAttributes(string taxonomy)
        {
            DataContext.Database.ExecuteSqlCommand(string.Format(CMD_DELETE_ALL_FATTRS, _prefix, taxonomy));
        }

        /// <summary>
        /// создаёт в БД атрибуты fcolor и fsize
        /// </summary>
        public void InstallCustomAttributes()
        {
            var attrs = DataContext.Set<WpWoocommerceAttributeTaxonomy>()
              .Where(x => x.attribute_name == WoocommerceAttributeName.FCOLOR || x.attribute_name == WoocommerceAttributeName.FSIZE)
              .AsNoTracking()
              .ToArray();

            var fcolorAttr = attrs.FirstOrDefault(x => x.attribute_name == WoocommerceAttributeName.FCOLOR);
            var fsizeAttr = attrs.FirstOrDefault(x => x.attribute_name == WoocommerceAttributeName.FSIZE);

            if (fcolorAttr == null)
            {
                DataContext.Add(WpWoocommerceAttributeTaxonomy.Create(WoocommerceAttributeName.FCOLOR));
            }
            if (fsizeAttr == null)
            {
                DataContext.Add(WpWoocommerceAttributeTaxonomy.Create(WoocommerceAttributeName.FSIZE));
            }
            DataContext.SaveChanges();
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
