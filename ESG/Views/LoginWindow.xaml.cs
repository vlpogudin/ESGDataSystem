using ESG.Data;
using ESG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ESG.Views
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
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

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var result = _dbService.AuthenticateUser(username, password);
            User user = result.user;
            string roleName = result.role_name;
            if (user != null)
            {
                AuthenticatedUser = user;
                RoleName = roleName;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль.", 
                    "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
