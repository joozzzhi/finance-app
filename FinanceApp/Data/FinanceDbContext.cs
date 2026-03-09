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

            // Пользователь
            modelBuilder.Entity<User>().HasData(new User
            {
                UserID = 2,
                Username = "123qwe",
                PasswordHash = BCryptHelper.HashPassword("1234"),
                Role = 0
            });

            // Демо-данные: расходы
            modelBuilder.Entity<Expense>().HasData(
                new Expense { ExpenseID = 1, UserID = 2, ExpenseName = "Продукты", ExpenseAmount = 3500.00m, ExpenseDate = new DateTime(2025, 3, 1) },
                new Expense { ExpenseID = 2, UserID = 2, ExpenseName = "Транспорт", ExpenseAmount = 1200.00m, ExpenseDate = new DateTime(2025, 3, 3) },
                new Expense { ExpenseID = 3, UserID = 2, ExpenseName = "Аренда квартиры", ExpenseAmount = 25000.00m, ExpenseDate = new DateTime(2025, 3, 5) },
                new Expense { ExpenseID = 4, UserID = 2, ExpenseName = "Интернет", ExpenseAmount = 600.00m, ExpenseDate = new DateTime(2025, 3, 5) },
                new Expense { ExpenseID = 5, UserID = 2, ExpenseName = "Кафе", ExpenseAmount = 1800.00m, ExpenseDate = new DateTime(2025, 3, 7) }
            );

            // Демо-данные: доходы
            modelBuilder.Entity<Income>().HasData(
                new Income { IncomeID = 1, UserID = 2, IncomeName = "Зарплата", IncomeAmount = 55000.00m, IncomeDate = new DateTime(2025, 3, 1) },
                new Income { IncomeID = 2, UserID = 2, IncomeName = "Фриланс", IncomeAmount = 15000.00m, IncomeDate = new DateTime(2025, 3, 10) },
                new Income { IncomeID = 3, UserID = 2, IncomeName = "Кэшбэк", IncomeAmount = 820.00m, IncomeDate = new DateTime(2025, 3, 12) }
            );

            // Демо-данные: финансовые цели
            modelBuilder.Entity<Target>().HasData(
                new Target { TargetID = 1, TargetName = "Отпуск", TargetAmount = 80000.00m, StartDate = new DateTime(2025, 1, 1), EndDate = new DateTime(2025, 7, 1) },
                new Target { TargetID = 2, TargetName = "Новый ноутбук", TargetAmount = 60000.00m, StartDate = new DateTime(2025, 2, 1), EndDate = new DateTime(2025, 6, 1) }
            );

            modelBuilder.Entity<UserTarget>().HasData(
                new UserTarget { UserTargetsID = 1, UserID = 2, TargetID = 1 },
                new UserTarget { UserTargetsID = 2, UserID = 2, TargetID = 2 }
            );
        }
    }
}
