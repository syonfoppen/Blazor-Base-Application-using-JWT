using Microsoft.EntityFrameworkCore;

namespace Recipe_Manager.Server.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options)
            : base(options)
        {
        }

        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Ingriedient> Ingriedients { get; set; }
        public DbSet<Step> Steps { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<IdentityRole> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().Navigation(e => e.Roles).AutoInclude();
            base.OnModelCreating(modelBuilder);

        }
    }
}