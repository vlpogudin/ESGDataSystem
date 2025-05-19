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
using System.ComponentModel;

namespace ESG.Views
{
    public partial class AddCompanyWindow : Window, INotifyPropertyChanged
    {
        private readonly Company _company;
        private readonly DatabaseService _dbService;
        private List<string> _availableIndustries;
        private string _industryFilter;
        private List<IndustryItem> _industryItems;

        public AddCompanyWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _company = new Company();

            // Загружаем список отраслей
            _availableIndustries = _dbService.GetIndustries().Select(i => i.IndustryName).ToList();
            _company.AvailableIndustries = new List<string>(_availableIndustries);

            // Создаём список элементов с флагом выбора
            _industryItems = _company.AvailableIndustries.Select(i => new IndustryItem { IndustryName = i, IsSelected = false }).ToList();
            FilteredIndustryItems = new List<IndustryItem>(_industryItems);

            DataContext = this;
            IndustriesListBox.DataContext = this; // Устанавливаем DataContext для ListBox отдельно
            _company.PropertyChanged += (s, e) => OnPropertyChanged(nameof(Company)); // Уведомляем об изменении Company
        }

        public Company Company
        {
            get => _company;
        }

        public string IndustryFilter
        {
            get => _industryFilter;
            set
            {
                _industryFilter = value;
                OnPropertyChanged(nameof(IndustryFilter));
                UpdateFilteredIndustries();
            }
        }

        private List<IndustryItem> _filteredIndustryItems;
        public List<IndustryItem> FilteredIndustryItems
        {
            get => _filteredIndustryItems;
            set
            {
                _filteredIndustryItems = value;
                OnPropertyChanged(nameof(FilteredIndustryItems));
            }
        }

        private void UpdateFilteredIndustries()
        {
            if (string.IsNullOrWhiteSpace(_industryFilter))
            {
                FilteredIndustryItems = new List<IndustryItem>(_industryItems);
            }
            else
            {
                FilteredIndustryItems = _industryItems
                    .Where(i => i.IndustryName.ToLower().Contains(_industryFilter.ToLower()))
                    .ToList();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_company.Name))
            {
                MessageBox.Show("Название компании обязательно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _company.SelectedIndustries = _industryItems
                .Where(i => i.IsSelected)
                .Select(i => i.IndustryName)
                .ToList();
            _dbService.AddCompany(_company);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}