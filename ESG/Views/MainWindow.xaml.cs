using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESG.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;
        private readonly string _roleName;
        private readonly MainViewModel _viewModel;

        public MainWindow(User currentUser, string roleName)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _roleName = roleName;
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Подписываемся на событие выхода
            _viewModel.LogoutRequested += ViewModel_LogoutRequested;

            UsersViewModel.CurrentUser = _currentUser;

            if (MainFrame == null)
            {
                MessageBox.Show("Ошибка: MainFrame не инициализирован.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainFrame.Navigate(new GreetingPage(_currentUser, MainFrame));
            ConfigureAccess();
        }

        private void ViewModel_LogoutRequested()
        {
            PerformLogout();
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
                    break;
                case "Модератор данных":
                case "Аналитик":
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

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LogoutCommand.Execute(null); // Вызываем команду выхода из ViewModel
        }

        private void PerformLogout()
        {
            try
            {
                // Создаем новое окно авторизации
                var loginWindow = new LoginWindow();
                // Показываем окно как диалоговое
                bool? loginResult = loginWindow.ShowDialog();

                if (loginResult == true)
                {
                    // Успешная авторизация — создаем новое окно MainWindow
                    var newMainWindow = new MainWindow(loginWindow.AuthenticatedUser, loginWindow.RoleName);
                    Application.Current.MainWindow = newMainWindow;
                    Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                    newMainWindow.Show();
                    // Закрываем текущее окно только после успешной авторизации
                    Close();
                }
                else
                {
                    // Авторизация отменена — просто возвращаемся, не завершая приложение
                    MessageBox.Show("Смена профиля отменена.",
                        "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Не вызываем Close() и Shutdown(), чтобы остаться в текущем окне
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выходе из профиля: {ex.Message}\n{ex.StackTrace}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                // Не завершаем приложение, чтобы пользователь мог продолжить работу
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Отписываемся от события, чтобы избежать утечек памяти
            _viewModel.LogoutRequested -= ViewModel_LogoutRequested;
        }
    }
}