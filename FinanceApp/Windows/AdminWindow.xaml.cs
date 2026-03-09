using System.Linq;
using System.Windows;
using FinanceApp.Data;
using FinanceApp.Models;

namespace FinanceApp.Windows
{
    public partial class AdminWindow : Window
    {
        private readonly User _currentUser;

        public AdminWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            AdminNameText.Text = $"Администратор: {_currentUser.Username}";
            LoadUsers();
            LoadStats();
        }

        private void LoadUsers()
        {
            using var db = new FinanceDbContext();
            var users = db.Users.Select(u => new
            {
                u.UserID,
                u.Username,
                RoleDisplay = u.Role == 1 ? "Администратор" : "Пользователь"
            }).ToList();

            UsersGrid.ItemsSource = users;
        }

        private void LoadStats()
        {
            using var db = new FinanceDbContext();

            TotalUsersText.Text = db.Users.Count().ToString();

            var totalIncomes = db.Incomes.Count();
            var totalExpenses = db.Expenses.Count();
            TotalTransactionsText.Text = (totalIncomes + totalExpenses).ToString();

            var allIncomes = db.Incomes.Sum(i => (decimal?)i.IncomeAmount) ?? 0;
            var allExpenses = db.Expenses.Sum(e => (decimal?)e.ExpenseAmount) ?? 0;
            AllIncomesText.Text = $"{allIncomes:N2} ₽";
            AllExpensesText.Text = $"{allExpenses:N2} ₽";
        }

        private void DeleteUser_Click(object sender, RoutedEventArgs e)
        {
            if (UsersGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для удаления!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = UsersGrid.SelectedItem;
            int userId = selected.UserID;

            if (userId == _currentUser.UserID)
            {
                MessageBox.Show("Нельзя удалить самого себя!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить пользователя \"{selected.Username}\" и все его данные?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var db = new FinanceDbContext();
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    var expenses = db.Expenses.Where(ex => ex.UserID == userId);
                    db.Expenses.RemoveRange(expenses);

                    var incomes = db.Incomes.Where(inc => inc.UserID == userId);
                    db.Incomes.RemoveRange(incomes);

                    var userTargets = db.UserTargets.Where(ut => ut.UserID == userId);
                    db.UserTargets.RemoveRange(userTargets);

                    db.Users.Remove(user);
                    db.SaveChanges();
                }

                LoadUsers();
                LoadStats();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
