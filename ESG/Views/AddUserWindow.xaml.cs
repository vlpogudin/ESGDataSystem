using ESG.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace ESG.Views
{
    public partial class AddUserWindow : Window
    {
        private readonly User _user;

        public AddUserWindow(User user, ObservableCollection<Role> roles)
        {
            InitializeComponent();
            _user = user;
            DataContext = _user;
            RoleComboBox.ItemsSource = roles;
        }

        public User User => _user;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_user.Username) || string.IsNullOrWhiteSpace(_user.Password) || _user.RoleId == 0)
            {
                MessageBox.Show("Все поля обязательны.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
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