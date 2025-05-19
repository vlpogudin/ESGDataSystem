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
    /// Логика взаимодействия для GreetingPage.xaml
    /// </summary>
    public partial class GreetingPage : Page
    {
        public GreetingPage(User currentUser, Frame mainFrame)
        {
            InitializeComponent();
            DataContext = new GreetingViewModel(currentUser, mainFrame);
        }
    }
}
