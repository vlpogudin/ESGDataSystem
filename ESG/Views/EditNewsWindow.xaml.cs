using ESG.Data;
using ESG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для EditNewsWindow.xaml
    /// </summary>
    public partial class EditNewsWindow : Window
    {
        private readonly News _news;
        private readonly DatabaseService _dbService;
        private ObservableCollection<Company> _companies;

        public EditNewsWindow(News news)
        {
            InitializeComponent();
            _news = news;
            _dbService = new DatabaseService();
            Companies = new ObservableCollection<Company>(_dbService.GetCompanies());

            var scrollViewer = Content as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.DataContext = _news;
            }
        }

        public ObservableCollection<Company> Companies
        {
            get => _companies;
            set => _companies = value;
        }

        public Company SelectedCompany
        {
            get => _news.CompanyId > 0 ? Companies.FirstOrDefault(c => c.CompanyId == _news.CompanyId) : null;
            set => _news.CompanyId = value?.CompanyId ?? 0;
        }

        public News News => _news;

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_news.Title))
            {
                MessageBox.Show("Пожалуйста, укажите заголовок новости.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_news.CompanyId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите компанию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
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
