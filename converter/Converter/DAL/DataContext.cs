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
                .WithOne()
                .HasForeignKey(x => x.object_id);

            modelBuilder.Entity<TermRelationship>()
                .ToTable("wp_term_relationships")
                .HasKey(x => new { x.object_id, x.term_taxonomy_id });
        }
    }
}
