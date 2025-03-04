using Microsoft.EntityFrameworkCore;
using CRM.Models.Entities;

namespace CRM.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Case> Cases { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //  Configure EmpID as a Foreign Key to the User table
            modelBuilder.Entity<Case>()
                .HasOne(c => c.User) // Case has one User
                .WithMany(u => u.Cases) // User can have many Cases
                .HasForeignKey(c => c.EmpID) // EmpID in Case references EmpID in User
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete
        }
    }
}