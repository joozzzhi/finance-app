using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceApp.Windows
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            UserNameText.Text = $"Пользователь: {_currentUser.Username}";
            ExpenseDatePicker.SelectedDate = DateTime.Today;
            IncomeDatePicker.SelectedDate = DateTime.Today;
            TargetEndDatePicker.SelectedDate = DateTime.Today.AddMonths(3);

            MainTabControl.SelectionChanged += TabControl_SelectionChanged;
            LoadDashboard();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                var tab = MainTabControl.SelectedIndex;
                switch (tab)
                {
                    case 0: LoadDashboard(); break;
                    case 1: LoadExpenses(); break;
                    case 2: LoadIncomes(); break;
                    case 3: LoadTargets(); break;
                }
            }
        }

        // ========== Общие данные ==========

        private void LoadDashboard()
        {
            using var db = new FinanceDbContext();

            var incomes = db.Incomes.Where(i => i.UserID == _currentUser.UserID).ToList();
            var expenses = db.Expenses.Where(e => e.UserID == _currentUser.UserID).ToList();

            var totalIncome = incomes.Sum(i => i.IncomeAmount);
            var totalExpense = expenses.Sum(e => e.ExpenseAmount);
            var balance = totalIncome - totalExpense;

            TotalIncomeText.Text = $"{totalIncome:N2} ₽";
            TotalExpenseText.Text = $"{totalExpense:N2} ₽";
            BalanceText.Text = $"{balance:N2} ₽";
            BalanceText.Foreground = balance >= 0
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(33, 150, 243))
                : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));

            var transactions = new List<dynamic>();
            foreach (var inc in incomes)
                transactions.Add(new { Type = "Доход", Name = inc.IncomeName, Amount = inc.IncomeAmount, Date = inc.IncomeDate });
            foreach (var exp in expenses)
                transactions.Add(new { Type = "Расход", Name = exp.ExpenseName, Amount = exp.ExpenseAmount, Date = exp.ExpenseDate });

            RecentTransactionsGrid.ItemsSource = transactions
                .OrderByDescending(t => t.Date)
                .Take(20)
                .ToList();
        }

        // ========== Расходы ==========

        private void LoadExpenses()
        {
            using var db = new FinanceDbContext();
            ExpensesGrid.ItemsSource = db.Expenses
                .Where(e => e.UserID == _currentUser.UserID)
                .OrderByDescending(e => e.ExpenseDate)
                .ToList();
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            var name = ExpenseNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название расхода!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(ExpenseAmountBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = ExpenseDatePicker.SelectedDate ?? DateTime.Today;

            using var db = new FinanceDbContext();
            db.Expenses.Add(new Expense
            {
                UserID = _currentUser.UserID,
                ExpenseName = name,
                ExpenseAmount = amount,
                ExpenseDate = date
            });
            db.SaveChanges();

            ExpenseNameBox.Clear();
            ExpenseAmountBox.Clear();
            ExpenseDatePicker.SelectedDate = DateTime.Today;
            LoadExpenses();
        }

        private void DeleteExpense_Click(object sender, RoutedEventArgs e)
        {
            if (ExpensesGrid.SelectedItem is not Expense selected)
            {
                MessageBox.Show("Выберите расход для удаления!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить расход \"{selected.ExpenseName}\" на сумму {selected.ExpenseAmount:N2} ₽?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var db = new FinanceDbContext();
                var expense = db.Expenses.Find(selected.ExpenseID);
                if (expense != null)
                {
                    db.Expenses.Remove(expense);
                    db.SaveChanges();
                }
                LoadExpenses();
            }
        }

        // ========== Доходы ==========

        private void LoadIncomes()
        {
            using var db = new FinanceDbContext();
            IncomesGrid.ItemsSource = db.Incomes
                .Where(i => i.UserID == _currentUser.UserID)
                .OrderByDescending(i => i.IncomeDate)
                .ToList();
        }

        private void AddIncome_Click(object sender, RoutedEventArgs e)
        {
            var name = IncomeNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название дохода!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(IncomeAmountBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var date = IncomeDatePicker.SelectedDate ?? DateTime.Today;

            using var db = new FinanceDbContext();
            db.Incomes.Add(new Income
            {
                UserID = _currentUser.UserID,
                IncomeName = name,
                IncomeAmount = amount,
                IncomeDate = date
            });
            db.SaveChanges();

            IncomeNameBox.Clear();
            IncomeAmountBox.Clear();
            IncomeDatePicker.SelectedDate = DateTime.Today;
            LoadIncomes();
        }

        private void DeleteIncome_Click(object sender, RoutedEventArgs e)
        {
            if (IncomesGrid.SelectedItem is not Income selected)
            {
                MessageBox.Show("Выберите доход для удаления!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить доход \"{selected.IncomeName}\" на сумму {selected.IncomeAmount:N2} ₽?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using var db = new FinanceDbContext();
                var income = db.Incomes.Find(selected.IncomeID);
                if (income != null)
                {
                    db.Incomes.Remove(income);
                    db.SaveChanges();
                }
                LoadIncomes();
            }
        }

        // ========== Финансовые цели ==========

        private void LoadTargets()
        {
            using var db = new FinanceDbContext();

            var userTargets = db.UserTargets
                .Where(ut => ut.UserID == _currentUser.UserID)
                .Include(ut => ut.Target)
                .Select(ut => ut.Target!)
                .ToList();

            var totalIncome = db.Incomes
                .Where(i => i.UserID == _currentUser.UserID)
                .Sum(i => (decimal?)i.IncomeAmount) ?? 0;

            var totalExpense = db.Expenses
                .Where(e => e.UserID == _currentUser.UserID)
                .Sum(e => (decimal?)e.ExpenseAmount) ?? 0;

            var savings = totalIncome - totalExpense;

            var targetViewModels = userTargets.Select(t => new
            {
                t.TargetID,
                t.TargetName,
                t.TargetAmount,
                t.EndDate,
                Progress = t.TargetAmount > 0
                    ? Math.Min(100, (double)(savings / t.TargetAmount) * 100)
                    : 0
            }).ToList();

            TargetsItemsControl.ItemsSource = targetViewModels;
        }

        private void AddTarget_Click(object sender, RoutedEventArgs e)
        {
            var name = TargetNameBox.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название цели!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(TargetAmountBox.Text.Replace(',', '.'),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var amount) || amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var endDate = TargetEndDatePicker.SelectedDate ?? DateTime.Today.AddMonths(3);

            using var db = new FinanceDbContext();
            var target = new Target
            {
                TargetName = name,
                TargetAmount = amount,
                StartDate = DateTime.Now,
                EndDate = endDate
            };
            db.Targets.Add(target);
            db.SaveChanges();

            db.UserTargets.Add(new UserTarget
            {
                UserID = _currentUser.UserID,
                TargetID = target.TargetID
            });
            db.SaveChanges();

            TargetNameBox.Clear();
            TargetAmountBox.Clear();
            TargetEndDatePicker.SelectedDate = DateTime.Today.AddMonths(3);
            LoadTargets();
        }

        private void DeleteTarget_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Выберите цель на карточке и нажмите удалить.",
                "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ========== Экспорт ==========

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"Финансовый_отчет_{DateTime.Now:yyyy-MM-dd}",
                    DefaultExt = ".docx",
                    Filter = "Word документ (.docx)|*.docx"
                };

                if (dialog.ShowDialog() == true)
                {
                    using var db = new FinanceDbContext();
                    var incomes = db.Incomes.Where(i => i.UserID == _currentUser.UserID).ToList();
                    var expenses = db.Expenses.Where(e => e.UserID == _currentUser.UserID).ToList();

                    WordExporter.Export(dialog.FileName, _currentUser.Username, incomes, expenses);

                    ExportStatusText.Text = "Данные успешно экспортированы!";
                    MessageBox.Show("Данные успешно экспортированы в Word!",
                        "Экспорт", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ========== Выход ==========

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }
    }
}
