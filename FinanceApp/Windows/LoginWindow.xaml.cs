using System.Linq;
using System.Windows;
using FinanceApp.Data;

namespace FinanceApp.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using var db = new FinanceDbContext();
            db.Database.EnsureCreated();

            var user = db.Users.FirstOrDefault(u => u.Username == username);

            if (user == null || !BCryptHelper.VerifyPassword(password, user.PasswordHash))
            {
                MessageBox.Show("Неправильный логин или пароль!", "Ошибка авторизации",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Авторизация прошла успешно!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            if (user.Role == 1)
            {
                var adminWindow = new AdminWindow(user);
                adminWindow.Show();
            }
            else
            {
                var mainWindow = new MainWindow(user);
                mainWindow.Show();
            }

            Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
    }
}
