using ESG.Data;
using ESG.Models;
using ESG.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ESG.Utilities; // Добавлено для PermissionChecker

namespace ESG.ViewModels
{
    public class NewsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private News _currentlySelectedNews;
        private string _companySearchText;
        private string _titleSearchText;
        private ObservableCollection<News> _allNews;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompaniesForList;
        private ObservableCollection<News> _filteredNewsForList;

        public NewsViewModel()
        {
            _dbService = new DatabaseService();
            _allNews = new ObservableCollection<News>(_dbService.GetNews(null, null, null, null));
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompaniesForList = new ObservableCollection<Company>(_allCompanies);
            _filteredNewsForList = new ObservableCollection<News>(_allNews);

            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
            }
            foreach (var news in _allNews)
            {
                news.IsSelected = false;
                _ = news.CompanyName;
            }

            FilteredNews = new ObservableCollection<News>(_allNews);
            SelectedCompanies = new ObservableCollection<Company>();
            SelectedNews = new ObservableCollection<News>();

            AddCommand = new RelayCommand(_ => AddNews(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditNews(), _ => CanPerformCrud && CurrentlySelectedNews != null);
            DeleteCommand = new RelayCommand(_ => DeleteNews(), _ => CanPerformCrud && CurrentlySelectedNews != null);
            ClearCompanySelectionCommand = new RelayCommand(_ => ClearCompanySelection());
            ClearTitleSelectionCommand = new RelayCommand(_ => ClearTitleSelection());
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
        }

        public ObservableCollection<News> FilteredNews { get; private set; }

        public ObservableCollection<Company> FilteredCompaniesForList
        {
            get => _filteredCompaniesForList;
            set
            {
                _filteredCompaniesForList = value;
                OnPropertyChanged(nameof(FilteredCompaniesForList));
            }
        }

        public ObservableCollection<News> FilteredNewsForList
        {
            get => _filteredNewsForList;
            set
            {
                _filteredNewsForList = value;
                OnPropertyChanged(nameof(FilteredNewsForList));
            }
        }

        public ObservableCollection<Company> SelectedCompanies { get; set; }

        public ObservableCollection<News> SelectedNews { get; set; }

        public News CurrentlySelectedNews
        {
            get => _currentlySelectedNews;
            set
            {
                _currentlySelectedNews = value;
                OnPropertyChanged(nameof(CurrentlySelectedNews));
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

        public string TitleSearchText
        {
            get => _titleSearchText;
            set
            {
                _titleSearchText = value;
                OnPropertyChanged(nameof(TitleSearchText));
                UpdateFilteredNewsForList();
            }
        }

        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearCompanySelectionCommand { get; }
        public ICommand ClearTitleSelectionCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        private void ClearCompanySelection()
        {
            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
            }
            SelectedCompanies.Clear();
            UpdateFilteredNews();
            UpdateFilteredNewsForList();
        }

        private void ClearTitleSelection()
        {
            foreach (var news in _allNews)
            {
                news.IsSelected = false;
            }
            SelectedNews.Clear();
            UpdateFilteredNews();
        }

        private void UpdateFilteredCompanies()
        {
            var filtered = _allCompanies.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                filtered = filtered.Where(c => c.Name?.ToLower().Contains(CompanySearchText.ToLower()) == true);
            }
            FilteredCompaniesForList.Clear();
            foreach (var company in filtered)
            {
                FilteredCompaniesForList.Add(company);
            }
        }

        public void UpdateFilteredNewsForList()
        {
            var filtered = _allNews.AsEnumerable();
            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(n => SelectedCompanies.Any(c => c.CompanyId == n.CompanyId));
            }
            if (!string.IsNullOrWhiteSpace(TitleSearchText))
            {
                filtered = filtered.Where(n => n.Title?.ToLower().Contains(TitleSearchText.ToLower()) == true);
            }
            FilteredNewsForList.Clear();
            foreach (var news in filtered)
            {
                FilteredNewsForList.Add(news);
            }
        }

        public void UpdateFilteredNews()
        {
            var filtered = _allNews.AsEnumerable();
            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(n => SelectedCompanies.Any(c => c.CompanyId == n.CompanyId));
            }
            if (SelectedNews.Any())
            {
                filtered = filtered.Where(n => SelectedNews.Any(s => s.Title == n.Title));
            }
            FilteredNews.Clear();
            foreach (var news in filtered)
            {
                FilteredNews.Add(news);
            }
            OnPropertyChanged(nameof(FilteredNews));
        }

        private void AddNews()
        {
            var window = new AddNewsWindow(new News(), this);
            if (window.ShowDialog() == true)
            {
                if (!window.IsCsvImported)
                {
                    var newNews = window.News;
                    newNews.CompanyName = _dbService.GetCompanyNameById(newNews.CompanyId) ?? "Неизвестная компания";
                    AddNewsToCollection(newNews);
                    FilteredNews.Add(newNews);
                    UpdateFilteredNews();
                    CurrentlySelectedNews = newNews;
                }
            }
        }

        private void EditNews()
        {
            var window = new EditNewsWindow(CurrentlySelectedNews);
            if (window.ShowDialog() == true)
            {
                CurrentlySelectedNews.CompanyName = _dbService.GetCompanyNameById(CurrentlySelectedNews.CompanyId) ?? "Неизвестная компания";
                _dbService.UpdateNews(CurrentlySelectedNews);
                UpdateFilteredNews();
            }
        }

        private void DeleteNews()
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить новость '{CurrentlySelectedNews.Title}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _dbService.DeleteNews(CurrentlySelectedNews.NewsId);
                _allNews.Remove(CurrentlySelectedNews);
                _filteredNewsForList.Remove(CurrentlySelectedNews);
                UpdateFilteredNews();
            }
        }

        private void ExportToCsv()
        {
            if (SelectedNews == null || !SelectedNews.Any())
            {
                MessageBox.Show("Выберите хотя бы одну новость для выгрузки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"NewsExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("ID,Компания,Заголовок,Содержание,Дата,Источник");

                    foreach (var news in SelectedNews)
                    {
                        var id = news.NewsId.ToString();
                        var companyName = $"\"{news.CompanyName?.Replace("\"", "\"\"") ?? ""}\"";
                        var title = $"\"{news.Title?.Replace("\"", "\"\"") ?? ""}\"";
                        var content = $"\"{news.Content?.Replace("\"", "\"\"") ?? ""}\"";
                        var date = news.Date?.ToString("yyyy-MM-dd") ?? "";
                        var source = $"\"{news.Source?.Replace("\"", "\"\"") ?? ""}\"";
                        csvBuilder.AppendLine($"{id},{companyName},{title},{content},{date},{source}");
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csvBuilder.ToString(), Encoding.UTF8);
                    MessageBox.Show("Данные успешно выгружены в CSV!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void AddNewsToCollection(News news)
        {
            if (!_allNews.Any(n => n.NewsId == news.NewsId))
            {
                _allNews.Add(news);
                _filteredNewsForList.Add(news);
            }
        }

        public void RefreshNewsFromDatabase()
        {
            _allNews.Clear();
            foreach (var news in _dbService.GetNews(null, null, null, null))
            {
                news.CompanyName = _dbService.GetCompanyNameById(news.CompanyId) ?? "Неизвестная компания";
                _allNews.Add(news);
            }
            UpdateFilteredNews();
            UpdateFilteredNewsForList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}