using ESG.Data;
using ESG.Models;
using ESG.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ESG.Utilities;

namespace ESG.ViewModels
{
    /// <summary>
    /// Управление списком новостей и операциями над ними
    /// </summary>
    public class NewsViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// // Сервис для взаимодействия с базой данных
        /// </summary>
        private readonly DatabaseService _dbService;

        /// <summary>
        /// Выбранная новость
        /// </summary>
        private News _currentlySelectedNews;

        /// <summary>
        /// Текст поиска для фильтрации компаний
        /// </summary>
        private string _companySearchText;

        /// <summary>
        /// Текст поиска для фильтрации новостей по заголовку
        /// </summary>
        private string _titleSearchText;

        /// <summary>
        /// Новости
        /// </summary>
        private ObservableCollection<News> _allNews;

        /// <summary>
        /// Компании
        /// </summary>
        private ObservableCollection<Company> _allCompanies;

        /// <summary>
        /// Отфильтрованные компании для списка
        /// </summary>
        private ObservableCollection<Company> _filteredCompaniesForList;

        /// <summary>
        /// Отфильтрованные новости для списка
        /// </summary>
        private ObservableCollection<News> _filteredNewsForList;

        #endregion

        #region Свойства

        /// <summary>
        /// Отфильтрованные новости
        /// </summary>
        public ObservableCollection<News> FilteredNews { get; private set; }

        /// <summary>
        /// Отфильтрованные новости для списка
        /// </summary>
        public ObservableCollection<Company> FilteredCompaniesForList
        {
            get => _filteredCompaniesForList;
            set
            {
                _filteredCompaniesForList = value;
                OnPropertyChanged(nameof(FilteredCompaniesForList));
            }
        }

        /// <summary>
        /// Отфильтрованные новости для списка
        /// </summary>
        public ObservableCollection<News> FilteredNewsForList
        {
            get => _filteredNewsForList;
            set
            {
                _filteredNewsForList = value;
                OnPropertyChanged(nameof(FilteredNewsForList));
            }
        }

        /// <summary>
        /// Выбранные компании
        /// </summary>
        public ObservableCollection<Company> SelectedCompanies { get; set; }

        /// <summary>
        /// Выбранные новости
        /// </summary>
        public ObservableCollection<News> SelectedNews { get; set; }

        /// <summary>
        /// Текущая новость
        /// </summary>
        public News CurrentlySelectedNews
        {
            get => _currentlySelectedNews;
            set
            {
                _currentlySelectedNews = value;
                OnPropertyChanged(nameof(CurrentlySelectedNews));
            }
        }

        /// <summary>
        /// Текст поиска для фильтрации компаний
        /// </summary>
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

        /// <summary>
        /// Текст поиска для фильтрации новостей по заголовку
        /// </summary>
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

        /// <summary>
        /// Проверка прав доступа для пользователя
        /// </summary>
        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();

        /// <summary>
        /// Добавление новости
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Редактирование новости
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// Удаление новости
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Очистка фильтров компаний
        /// </summary>
        public ICommand ClearCompanySelectionCommand { get; }

        /// <summary>
        /// Очистка фильтров новостей
        /// </summary>
        public ICommand ClearTitleSelectionCommand { get; }

        /// <summary>
        /// Выгрузка данных в CSV файл
        /// </summary>
        public ICommand ExportToCsvCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// инициализация VM
        /// </summary>
        public NewsViewModel()
        {
            _dbService = new DatabaseService();

            // Загрузка всех новостей и компаний
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
            ClearCompanySelectionCommand = new RelayCommand(_ => ClearCompanyFilter());
            ClearTitleSelectionCommand = new RelayCommand(_ => ClearTitleFilter());
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
        }

        #endregion

        #region Методы

        /// <summary>
        /// Очистка выбора компаний
        /// </summary>
        private void ClearCompanyFilter()
        {
            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
            }
            SelectedCompanies.Clear();
            UpdateFilteredNews();
            UpdateFilteredNewsForList();
        }

        /// <summary>
        /// Очистка выбора новостей
        /// </summary>
        private void ClearTitleFilter()
        {
            foreach (var news in _allNews)
            {
                news.IsSelected = false;
            }
            SelectedNews.Clear();
            UpdateFilteredNews();
        }

        /// <summary>
        /// Обновление списка отфильтрованных компаний
        /// </summary>
        private void UpdateFilteredCompanies()
        {
            // Фильтрация компаний по тексту поиска
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

        /// <summary>
        /// Обновление списка отфильтрованных новостей для отображения
        /// </summary>
        public void UpdateFilteredNewsForList()
        {
            // Фильтрация новостей по выбранным компаниям и тексту поиска
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

        /// <summary>
        /// Обновление списка отфильтрованных новостей
        /// </summary>
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

        /// <summary>
        /// Добавление новости
        /// </summary>
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

        /// <summary>
        /// Редактирование новости
        /// </summary>
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

        /// <summary>
        /// Удаление новости
        /// </summary>
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

        /// <summary>
        /// Выгрузка данных в CSV файл
        /// </summary>
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

        /// <summary>
        /// Добавление новости в коллекции
        /// </summary>
        /// <param name="news"></param>
        public void AddNewsToCollection(News news)
        {
            if (!_allNews.Any(n => n.NewsId == news.NewsId))
            {
                _allNews.Add(news);
                _filteredNewsForList.Add(news);
            }
        }

        /// <summary>
        /// Обновление новостей из базы данных
        /// </summary>
        public void RefreshNews()
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

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод для вызова события изменения свойства
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}