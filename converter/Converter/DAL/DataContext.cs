using Converter.DAL.Constants;
using Converter.DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace Converter.DAL
{

    public class DataContext : DbContext
    {
        private readonly string _tablePrefix;
        private readonly string _connectionString;

        public DataContext(string tablePrefix, string connectionString)
        {
            _tablePrefix = tablePrefix;
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var postBuilder = modelBuilder.Entity<Post>();
            postBuilder
                .ToTable($"{_tablePrefix}{Tables.Posts}")
                .HasKey(x => x.ID);

            postBuilder
                .HasMany(x => x.TermRelationships)
                .WithOne()
                .HasForeignKey(x => x.object_id);
            postBuilder
                .Ignore(x => x.Colours)
                .Ignore(x => x.FColours)
                .Ignore(x => x.Sizes)
                .Ignore(x => x.FSizes)
                .Ignore(x => x.Categories);

            var termRelationship = modelBuilder.Entity<TermRelationship>();
                termRelationship
                .ToTable($"{_tablePrefix}{Tables.TermRelationships}")
                .HasKey(x => new { x.object_id, x.term_taxonomy_id });

            termRelationship
                .HasOne(x => x.TermTaxonomy)
                .WithMany()
                .HasForeignKey(x => x.term_taxonomy_id);

            modelBuilder.Entity<Term>()
                .ToTable($"{_tablePrefix}{Tables.Terms}")
                .HasKey(x => x.term_id);

            var termTaxonomyBuilder = modelBuilder.Entity<TermTaxonomy>();
                termTaxonomyBuilder
                .ToTable($"{_tablePrefix}{Tables.TermTaxonomy}")
                .HasKey(t => t.term_taxonomy_id);

            termTaxonomyBuilder
                .HasOne(x => x.Term)
                .WithMany()
                .HasForeignKey(x => x.term_id);

            modelBuilder.Entity<TaxonomyWithCount>()
                .HasKey(x => x.term_taxonomy_id);

            modelBuilder.Entity<WpWoocommerceAttributeTaxonomy>()
               .ToTable($"{_tablePrefix}{Tables.WpWoocommerceAttributeTaxonomies}")
               .HasKey(x => x.attribute_id);
        }
    }
}
