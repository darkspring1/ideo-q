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
                .ToTable("wp_posts")
                .HasKey(x => x.ID);

            postBuilder
                .HasMany(x => x.TermRelationships)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.object_id);
            postBuilder
                .Ignore(x => x.Colours)
                .Ignore(x => x.FColours);

            var termRelationship = modelBuilder.Entity<TermRelationship>();
                termRelationship
                .ToTable("wp_term_relationships")
                .HasKey(x => new { x.object_id, x.term_taxonomy_id });

            termRelationship
                .HasOne(x => x.TermTaxonomy)
                .WithOne()
                .HasForeignKey<TermRelationship>(x => x.term_taxonomy_id);

            modelBuilder.Entity<Term>()
                .ToTable("wp_terms")
                .HasKey(x => x.term_id);

            var termTaxonomyBuilder = modelBuilder.Entity<TermTaxonomy>();
                termTaxonomyBuilder
                .ToTable("wp_term_taxonomy")
                .HasKey(t => t.term_taxonomy_id);

            termTaxonomyBuilder
                .HasOne(x => x.Term)
                .WithOne()
                .HasForeignKey<TermTaxonomy>(x => x.term_id);
        }
    }
}
