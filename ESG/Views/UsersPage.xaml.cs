using ESG.Models;
using ESG.ViewModels;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ESG.Views
{
    /// <summary>
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public UsersPage()
        {
            InitializeComponent();
            DataContext = new UsersViewModel();
        }

        private void UserCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is User user)
            {
                viewModel.AddSelectedUser(user);
            }
        }

        private void UserCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsersViewModel viewModel && sender is CheckBox checkBox && checkBox.DataContext is User user)
            {
                viewModel.RemoveSelectedUser(user);
            }
        }
    }
}
