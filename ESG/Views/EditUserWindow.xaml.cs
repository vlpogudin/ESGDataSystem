using ESG.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ESG.Views
{
    public partial class EditUserWindow : Window
    {
        private readonly User _user;

        public EditUserWindow(User user, ObservableCollection<Role> roles)
        {
            InitializeComponent();
            _user = user;
            DataContext = _user;
            RoleComboBox.ItemsSource = roles;
        }

        public User User => _user;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_user.Username) || _user.RoleId == 0)
            {
                MessageBox.Show("Логин и роль обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Если пароль не введен, оставляем старый хэш
            if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                _user.Password = PasswordBox.Password; // Новый пароль будет хэширован в UpdateUser
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}