using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly string _roleName;

        public MainWindow(User currentUser, string roleName)
        {
            try
            {
                InitializeComponent();
                _currentUser = currentUser;
                _roleName = roleName;

                // Устанавливаем текущего пользователя в UsersViewModel
                UsersViewModel.CurrentUser = _currentUser;

                System.Diagnostics.Debug.WriteLine($"MainFrame: {MainFrame != null}");
                System.Diagnostics.Debug.WriteLine($"ExportDataMenuItem: {ExportDataMenuItem != null}");

                if (MainFrame == null)
                {
                    MessageBox.Show("Ошибка: MainFrame не инициализирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MainFrame.Navigate(new GreetingPage(_currentUser, MainFrame));
                ConfigureAccess();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка при создании MainWindow: {ex.Message}\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current?.Shutdown();
            }
        }

        private void ConfigureAccess()
        {
            if (_roleName == null)
            {
                MessageBox.Show("Ошибка: Роль не определена.", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            switch (_roleName)
            {
                case "Администратор":
                    // Полный доступ
                    break;

                case "Модератор данных":
                    // Скрываем только "Пользователи"
                    UsersMenuItem.Visibility = Visibility.Collapsed;
                    break;

                case "Аналитик":
                    // Скрываем только "Пользователи"
                    UsersMenuItem.Visibility = Visibility.Collapsed;
                    break;

                default:
                    MessageBox.Show($"Неизвестная роль: {_roleName}. Доступ ограничен.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    CompaniesMenuItem.Visibility = Visibility.Collapsed;
                    ReportsMenuItem.Visibility = Visibility.Collapsed;
                    WebsitesMenuItem.Visibility = Visibility.Collapsed;
                    NewsMenuItem.Visibility = Visibility.Collapsed;
                    IndustriesMenuItem.Visibility = Visibility.Collapsed;
                    UsersMenuItem.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void HomeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GreetingPage(_currentUser, MainFrame));
        }

        private void ExportDataMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ExportDataPage());
        }

        private void CompaniesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CompaniesPage());
        }

        private void ReportsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ReportsPage());
        }

        private void WebsitesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new WebsitesPage());
        }

        private void NewsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new NewsPage());
        }

        private void IndustriesMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new IndustriesPage());
        }

        private void UsersMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }
    }
}