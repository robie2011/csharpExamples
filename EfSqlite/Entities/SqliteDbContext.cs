using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace EfSqlite.Entities
{
    public class SqliteDbContext : DbContext
    {
        public SqliteDbContext() 
            : base()
        {
        }
        
        // DbSet<T> will be auto discovered by EF, added as entity and property name is used to map as table name
        // https://www.learnentityframeworkcore.com/dbset
        public DbSet<Tree> Tree { get; set; }
        
        public DbSet<Leaf> Leaf { get; set; }
        
        public DbSet<Quote> Quote { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var modelBuilder = new ModelBuilder(SqliteConventionSetBuilder.Build());
            modelBuilder.Entity<Tree>();

            optionsBuilder
                .UseSqlite(@"DataSource=mydatabase.db")
                .UseSnakeCaseNamingConvention();
        }
    }
}