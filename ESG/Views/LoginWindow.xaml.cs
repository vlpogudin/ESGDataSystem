using ESG.Data;
using ESG.Models;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService;
        public User AuthenticatedUser { get; private set; }
        public string RoleName { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var (user, roleName) = _dbService.AuthenticateUser(username, password);

            if (user != null)
            {
                AuthenticatedUser = user;
                RoleName = roleName;
                DialogResult = true; // Устанавливаем результат диалога как успешный
                Close(); // Закрываем окно
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.",
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Устанавливаем результат диалога как неуспешный
            Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Не завершаем приложение здесь, так как это делает App.xaml.cs
        }
    }
}