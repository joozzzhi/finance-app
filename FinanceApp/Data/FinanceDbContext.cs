using Microsoft.EntityFrameworkCore;
using FinanceApp.Models;

namespace FinanceApp.Data
{
    public class FinanceDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Target> Targets { get; set; }
        public DbSet<UserTarget> UserTargets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\MSSQLLocalDB;Database=FinanceDB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Expense>()
                .HasOne(e => e.User)
                .WithMany(u => u.Expenses)
                .HasForeignKey(e => e.UserID);

            modelBuilder.Entity<Income>()
                .HasOne(i => i.User)
                .WithMany(u => u.Incomes)
                .HasForeignKey(i => i.UserID);

            modelBuilder.Entity<UserTarget>()
                .HasOne(ut => ut.User)
                .WithMany(u => u.UserTargets)
                .HasForeignKey(ut => ut.UserID);

            modelBuilder.Entity<UserTarget>()
                .HasOne(ut => ut.Target)
                .WithMany(t => t.UserTargets)
                .HasForeignKey(ut => ut.TargetID);

            // Начальные данные - администратор
            modelBuilder.Entity<User>().HasData(new User
            {
                UserID = 1,
                Username = "admin",
                PasswordHash = BCryptHelper.HashPassword("admin123"),
                Role = 1
            });
        }
    }
}
