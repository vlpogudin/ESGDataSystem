using ESG.Data;
using ESG.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class EditWebsiteWindow : Window, INotifyPropertyChanged
    {
        private readonly Website _website;
        private readonly DatabaseService _dbService;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompanies;
        private string _companySearchText;

        public EditWebsiteWindow(Website website)
        {
            InitializeComponent();
            _website = website ?? new Website();
            _dbService = new DatabaseService();
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompanies = new ObservableCollection<Company>(_allCompanies);

            // Устанавливаем начальное значение IsSelected для текущей компании
            if (_website.CompanyId > 0)
            {
                var currentCompany = _allCompanies.FirstOrDefault(c => c.CompanyId == _website.CompanyId);
                if (currentCompany != null)
                {
                    currentCompany.IsSelected = true;
                }
            }
            else
            {
                foreach (var company in _allCompanies)
                {
                    company.IsSelected = false;
                }
            }

            var scrollViewer = Content as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.DataContext = this;
                if (_website.LastUpdated == null)
                {
                    _website.LastUpdated = DateTime.Now;
                }
            }

            DataContext = this;
        }

        public ObservableCollection<Company> FilteredCompanies
        {
            get => _filteredCompanies;
            set
            {
                _filteredCompanies = value;
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                _companySearchText = value;
                OnPropertyChanged(nameof(CompanySearchText));
                UpdateFilteredCompanies();
            }
        }

        public Website Website => _website;

        private void UpdateFilteredCompanies()
        {
            var filtered = _allCompanies.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                filtered = filtered.Where(c => c.Name?.ToLower().Contains(CompanySearchText.ToLower()) == true);
            }

            FilteredCompanies.Clear();
            foreach (var company in filtered)
            {
                FilteredCompanies.Add(company);
            }
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                // Снимаем выбор с других компаний
                foreach (var c in _allCompanies.Where(c => c != company))
                {
                    c.IsSelected = false;
                }
                company.IsSelected = true;
                _website.CompanyId = company.CompanyId;
                UpdateFilteredCompanies(); // Обновляем список
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                company.IsSelected = false;
                if (company.CompanyId == _website.CompanyId)
                {
                    _website.CompanyId = 0;
                }
                UpdateFilteredCompanies();
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_website.Url))
            {
                MessageBox.Show("Пожалуйста, укажите URL.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_website.CompanyId <= 0)
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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}