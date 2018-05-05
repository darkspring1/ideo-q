using Converter.DAL.Constants;
using Converter.DAL.Entity;
using Microsoft.EntityFrameworkCore;

namespace Converter.DAL
{

    public class DataContext : DbContext
    {
        private readonly string connectionString;

        public DataContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var postBuilder = modelBuilder.Entity<Post>();
            postBuilder
                .ToTable(Tables.Posts)
                .HasKey(x => x.ID);

            postBuilder
                .HasMany(x => x.TermRelationships)
                .WithOne()
                .HasForeignKey(x => x.object_id);
            postBuilder
                .Ignore(x => x.Colours)
                .Ignore(x => x.FColours);

            var termRelationship = modelBuilder.Entity<TermRelationship>();
                termRelationship
                .ToTable(Tables.TermRelationships)
                .HasKey(x => new { x.object_id, x.term_taxonomy_id });

            termRelationship
                .HasOne(x => x.TermTaxonomy)
                .WithMany()
                .HasForeignKey(x => x.term_taxonomy_id);

            modelBuilder.Entity<Term>()
                .ToTable(Tables.Terms)
                .HasKey(x => x.term_id);

            var termTaxonomyBuilder = modelBuilder.Entity<TermTaxonomy>();
                termTaxonomyBuilder
                .ToTable(Tables.TermTaxonomy)
                .HasKey(t => t.term_taxonomy_id);

            termTaxonomyBuilder
                .HasOne(x => x.Term)
                .WithMany()
                .HasForeignKey(x => x.term_id);
        }
    }
}
