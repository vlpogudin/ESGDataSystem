using ESG.Data;
using ESG.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ESG.ViewModels;

namespace ESG.Views
{
    public partial class AddWebsiteWindow : Window, INotifyPropertyChanged
    {
        private Website _website;
        private readonly DatabaseService _dbService;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompanies;
        private string _companySearchText;
        private readonly WebsitesViewModel _viewModel;
        public bool IsCsvImported { get; private set; } // Флаг для отслеживания импорта CSV

        public AddWebsiteWindow(Website website, WebsitesViewModel viewModel = null)
        {
            InitializeComponent();
            _website = website ?? new Website();
            _dbService = new DatabaseService();
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompanies = new ObservableCollection<Company>(_allCompanies);
            _viewModel = viewModel;
            IsCsvImported = false; // Изначально флаг false

            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
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

        public Website Website
        {
            get => _website;
            set
            {
                if (_website != value)
                {
                    _website = value;
                    OnPropertyChanged(nameof(Website));
                }
            }
        }

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
                foreach (var c in _allCompanies.Where(c => c != company))
                {
                    c.IsSelected = false;
                }
                company.IsSelected = true;
                _website.CompanyId = company.CompanyId;
                UpdateFilteredCompanies();
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

            // Сохраняем веб-сайт в базе данных
            _website = _dbService.AddWebsite(_website); // Обновляем объект с правильным WebsiteId

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ImportCsv_Click(object sender, RoutedEventArgs e)
        {
            var instructionWindow = new ImportCsvInstructionWindow(this);
            if (instructionWindow.ShowDialog() == true)
            {
                if (_viewModel != null && instructionWindow.AddedOrUpdatedWebsites.Any())
                {
                    foreach (var website in instructionWindow.AddedOrUpdatedWebsites)
                    {
                        _viewModel.AddWebsiteToCollection(website);
                    }
                    // Принудительное обновление из базы данных
                    _viewModel.RefreshWebsitesFromDatabase();
                    IsCsvImported = true; // Устанавливаем флаг, что импорт CSV выполнен
                }

                MessageBox.Show(instructionWindow.StatusMessage, "Результат загрузки", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}