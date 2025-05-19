using ESG.Data;
using ESG.Models;
using ESG.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class AddNewsWindow : Window, INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private readonly NewsViewModel _viewModel;
        private string _companySearchText;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompanies;

        public AddNewsWindow(News news, NewsViewModel viewModel)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _viewModel = viewModel;
            News = news ?? new News();
            DataContext = this;

            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompanies = new ObservableCollection<Company>(_allCompanies);
            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
                if (News.CompanyId != 0 && company.CompanyId == News.CompanyId)
                {
                    company.IsSelected = true;
                }
            }

            IsCsvImported = false;
        }

        public News News { get; private set; }
        public bool IsCsvImported { get; private set; }

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
                foreach (var c in _allCompanies)
                {
                    c.SetIsSelectedWithoutNotification(false);
                }
                company.IsSelected = true;
                News.CompanyId = company.CompanyId;
                FilteredCompanies.Clear();
                foreach (var c in _allCompanies)
                {
                    FilteredCompanies.Add(c);
                }
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                company.IsSelected = false;
                News.CompanyId = 0;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(News.Title) || News.CompanyId == 0)
            {
                MessageBox.Show("Заполните обязательные поля: 'Заголовок' и 'Компания'.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (News.NewsId == 0)
            {
                News = _dbService.AddNews(News); // Обновляем объект с правильным NewsId
                _viewModel.AddNewsToCollection(News); // Добавляем в основную коллекцию
                if (!_viewModel.FilteredNews.Any(n => n.NewsId == News.NewsId)) // Проверяем дублирование
                {
                    _viewModel.FilteredNews.Add(News); // Добавляем в отображаемую коллекцию
                }
            }
            else
            {
                _dbService.UpdateNews(News);
            }

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
            var importWindow = new ImportCsvNewsWindow(this);
            if (importWindow.ShowDialog() == true)
            {
                IsCsvImported = true;
                foreach (var news in importWindow.AddedOrUpdatedNews)
                {
                    // Проверяем, нет ли новости с таким же NewsId
                    if (!_viewModel.FilteredNews.Any(n => n.NewsId == news.NewsId))
                    {
                        _viewModel.AddNewsToCollection(news);
                        _viewModel.FilteredNews.Add(news);
                    }
                }
                _viewModel.RefreshNews(); // Обновляем данные
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