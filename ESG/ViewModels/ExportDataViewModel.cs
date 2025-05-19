using ESG.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using ESG.Data;

namespace ESG.ViewModels
{
    /// <summary>
    /// Выгрузка отфильтрованных данных
    /// </summary>
    public class ExportDataViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Сервис для взаимодействия с базой данных
        /// </summary>
        private readonly DatabaseService _dbService;

        /// <summary>
        /// Отрасли
        /// </summary>
        private ObservableCollection<string> _industries;

        /// <summary>
        /// Отфильтрованные отрасли
        /// </summary>
        private ObservableCollection<string> _filteredIndustries;

        /// <summary>
        /// Выбранные отрасли
        /// </summary>
        private ObservableCollection<string> _selectedIndustries;

        /// <summary>
        /// Компании
        /// </summary>
        private ObservableCollection<Company> _allCompanies;

        /// <summary>
        /// Доступные компании
        /// </summary>
        private ObservableCollection<Company> _availableCompanies;

        /// <summary>
        /// Отфильтрованные компании
        /// </summary>
        private ObservableCollection<Company> _filteredCompanies;

        /// <summary>
        /// Выбранные компании
        /// </summary>
        private ObservableCollection<Company> _selectedCompanies;

        /// <summary>
        /// Отчеты
        /// </summary>
        private ObservableCollection<string> _reports;

        /// <summary>
        /// Отфильтрованные отчеты
        /// </summary>
        private ObservableCollection<string> _filteredReports;

        /// <summary>
        /// Выбранные отчеты
        /// </summary>
        private ObservableCollection<string> _selectedReports;

        /// <summary>
        /// Новости
        /// </summary>
        private ObservableCollection<string> _news;

        /// <summary>
        /// Отфильтрованные новости
        /// </summary>
        private ObservableCollection<string> _filteredNews;

        /// <summary>
        /// Выбранные новости
        /// </summary>
        private ObservableCollection<string> _selectedNews;

        /// <summary>
        /// Начало периода
        /// </summary>
        private int? _startYear;

        /// <summary>
        /// Конец периода
        /// </summary>
        private int? _endYear;

        /// <summary>
        /// Текст для поиска отраслей
        /// </summary>
        private string _industrySearchText;

        /// <summary>
        /// Текст для поиска компаний
        /// </summary>
        private string _companySearchText;

        /// <summary>
        /// Текст для поиска отчетов
        /// </summary>
        private string _reportSearchText;

        /// <summary>
        /// Текст для поиска новостей
        /// </summary>
        private string _newsSearchText;

        /// <summary>
        /// Сводка компаний по выбранным фильтрам
        /// </summary>
        private ObservableCollection<CompanySummary> _filteredSummaries;

        #endregion

        #region Свойства

        /// <summary>
        /// Отображение года, с которого происходит выбор в календаре
        /// </summary>
        public ObservableCollection<int?> YearsStart { get; }

        /// <summary>
        /// Отображение года, до которого происходит выбор в календаре
        /// </summary>
        public ObservableCollection<int?> YearsEnd { get; }

        /// <summary>
        /// Отрасли
        /// </summary>
        public ObservableCollection<string> Industries
        {
            get => _industries;
            set
            {
                _industries = value;
                OnPropertyChanged(nameof(Industries));
            }
        }

        /// <summary>
        /// Отфильтрованные отрасли
        /// </summary>
        public ObservableCollection<string> FilteredIndustries
        {
            get => _filteredIndustries;
            set
            {
                _filteredIndustries = value;
                OnPropertyChanged(nameof(FilteredIndustries));
            }
        }

        /// <summary>
        /// Выбранные отрасли
        /// </summary>
        public ObservableCollection<string> SelectedIndustries
        {
            get => _selectedIndustries;
            set
            {
                _selectedIndustries = value;
                OnPropertyChanged(nameof(SelectedIndustries));
            }
        }

        /// <summary>
        /// Компании
        /// </summary>
        public ObservableCollection<Company> AllCompanies
        {
            get => _allCompanies;
            set
            {
                _allCompanies = value;
                OnPropertyChanged(nameof(AllCompanies));
            }
        }

        /// <summary>
        /// Доступные компании
        /// </summary>
        public ObservableCollection<Company> AvailableCompanies
        {
            get => _availableCompanies;
            set
            {
                _availableCompanies = value;
                OnPropertyChanged(nameof(AvailableCompanies));
            }
        }

        /// <summary>
        /// Отфильтрованные компании
        /// </summary>
        public ObservableCollection<Company> FilteredCompanies
        {
            get => _filteredCompanies;
            set
            {
                _filteredCompanies = value;
                OnPropertyChanged(nameof(FilteredCompanies));
            }
        }

        /// <summary>
        /// Выбранные компании
        /// </summary>
        public ObservableCollection<Company> SelectedCompanies
        {
            get => _selectedCompanies;
            set
            {
                _selectedCompanies = value;
                OnPropertyChanged(nameof(SelectedCompanies));
            }
        }

        /// <summary>
        /// Отчеты
        /// </summary>
        public ObservableCollection<string> Reports
        {
            get => _reports;
            set
            {
                _reports = value;
                OnPropertyChanged(nameof(Reports));
            }
        }

        /// <summary>
        /// Отфильтрованные отчеты
        /// </summary>
        public ObservableCollection<string> FilteredReports
        {
            get => _filteredReports;
            set
            {
                _filteredReports = value;
                OnPropertyChanged(nameof(FilteredReports));
            }
        }

        /// <summary>
        /// Выбранные отчеты
        /// </summary>
        public ObservableCollection<string> SelectedReports
        {
            get => _selectedReports;
            set
            {
                _selectedReports = value;
                OnPropertyChanged(nameof(SelectedReports));
            }
        }

        /// <summary>
        /// Новости
        /// </summary>
        public ObservableCollection<string> News
        {
            get => _news;
            set
            {
                _news = value;
                OnPropertyChanged(nameof(News));
            }
        }

        /// <summary>
        /// Отфильтрованные новости
        /// </summary>
        public ObservableCollection<string> FilteredNews
        {
            get => _filteredNews;
            set
            {
                _filteredNews = value;
                OnPropertyChanged(nameof(FilteredNews));
            }
        }

        /// <summary>
        /// Выбранные новости
        /// </summary>
        public ObservableCollection<string> SelectedNews
        {
            get => _selectedNews;
            set
            {
                _selectedNews = value;
                OnPropertyChanged(nameof(SelectedNews));
            }
        }

        /// <summary>
        /// Начало периода
        /// </summary>
        public int? StartYear
        {
            get => _startYear;
            set
            {
                _startYear = value;
                OnPropertyChanged(nameof(StartYear));
            }
        }

        /// <summary>
        /// Конец периода
        /// </summary>
        public int? EndYear
        {
            get => _endYear;
            set
            {
                _endYear = value;
                OnPropertyChanged(nameof(EndYear));
            }
        }

        /// <summary>
        /// Текст для поиска отраслей
        /// </summary>
        public string IndustrySearchText
        {
            get => _industrySearchText;
            set
            {
                _industrySearchText = value;
                OnPropertyChanged(nameof(IndustrySearchText));
            }
        }

        /// <summary>
        /// Текст для поиска компаний
        /// </summary>
        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                _companySearchText = value;
                OnPropertyChanged(nameof(CompanySearchText));
            }
        }

        /// <summary>
        /// Текст для поиска отчетов
        /// </summary>
        public string ReportSearchText
        {
            get => _reportSearchText;
            set
            {
                _reportSearchText = value;
                OnPropertyChanged(nameof(ReportSearchText));
            }
        }

        /// <summary>
        /// Текст для поиска новостей
        /// </summary>
        public string NewsSearchText
        {
            get => _newsSearchText;
            set
            {
                _newsSearchText = value;
                OnPropertyChanged(nameof(NewsSearchText));
            }
        }

        /// <summary>
        /// Сводка данных по выбранным фильтрам
        /// </summary>
        public ObservableCollection<CompanySummary> FilteredSummaries
        {
            get => _filteredSummaries;
            set
            {
                _filteredSummaries = value;
                OnPropertyChanged(nameof(FilteredSummaries));
            }
        }

        /// <summary>
        /// Команда экспорта данных
        /// </summary>
        public ICommand ExportToCsvCommand { get; }

        /// <summary>
        /// Команда очистки всех фильтров
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация VM
        /// </summary>
        public ExportDataViewModel()
        {
            _dbService = new DatabaseService(); // Инициализация сервиса базы данных

            // Загрузка отраслей из базы данных
            Industries = new ObservableCollection<string>(_dbService.GetIndustries().Select(i => i.IndustryName));
            FilteredIndustries = new ObservableCollection<string>(Industries);
            SelectedIndustries = new ObservableCollection<string>();

            // Загрузка отраслей из базы данных
            AllCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            AvailableCompanies = new ObservableCollection<Company>(AllCompanies);
            FilteredCompanies = new ObservableCollection<Company>(AvailableCompanies);
            SelectedCompanies = new ObservableCollection<Company>();

            // Загрузка отчетов из базы данных
            Reports = new ObservableCollection<string>(_dbService.GetReports(null, null, null, null).Select(r => r.Title));
            FilteredReports = new ObservableCollection<string>(Reports);
            SelectedReports = new ObservableCollection<string>();

            // Загрузка новостей из базы данных
            News = new ObservableCollection<string>(_dbService.GetNews(null, null, null, null).Select(n => n.Title));
            FilteredNews = new ObservableCollection<string>(News);
            SelectedNews = new ObservableCollection<string>();

            // Инициализация списка с начальными годами
            YearsStart = new ObservableCollection<int?>
            {
                null,
            };
            for (int year = 2000; year <= DateTime.Now.Year; year++)
            {
                YearsStart.Add(year);
            }

            // Инициализация списка с конечномы годами по убыванию
            YearsEnd = new ObservableCollection<int?>
            {
                null,
            };
            for (int year = DateTime.Now.Year; year >= 2000; year--)
            {
                YearsEnd.Add(year);
            }

            StartYear = null;
            EndYear = null;
            IndustrySearchText = string.Empty;
            CompanySearchText = string.Empty;
            ReportSearchText = string.Empty;
            NewsSearchText = string.Empty;

            FilteredSummaries = new ObservableCollection<CompanySummary>();
            ExportToCsvCommand = new RelayCommand(ExportToCsv);
            ClearFiltersCommand = new RelayCommand(ClearFilters);

            // Инициализация таблицы всеми данными
            UpdateFilteredSummaries();

            SelectedIndustries.CollectionChanged += (s, e) => UpdateAvailableCompanies();
            SelectedCompanies.CollectionChanged += (s, e) => UpdateFilteredSummaries();
            SelectedReports.CollectionChanged += (s, e) => UpdateFilteredSummaries();
            SelectedNews.CollectionChanged += (s, e) => UpdateFilteredSummaries();
            // Подписка на изменения свойств для обработки поиска и фильтров
            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IndustrySearchText))
                {
                    UpdateFilteredIndustries();
                }
                else if (args.PropertyName == nameof(CompanySearchText))
                {
                    UpdateFilteredCompanies();
                }
                else if (args.PropertyName == nameof(ReportSearchText))
                {
                    UpdateFilteredReports();
                }
                else if (args.PropertyName == nameof(NewsSearchText))
                {
                    UpdateFilteredNews();
                }
                else if (args.PropertyName == nameof(StartYear) ||
                         args.PropertyName == nameof(EndYear))
                {
                    UpdateFilteredSummaries();
                }
            };
        }

        #endregion

        #region Методы

        /// <summary>
        /// // Обновление отфильтрованных отраслей по тексту в фильтре
        /// </summary>
        private void UpdateFilteredIndustries()
        {
            var filtered = Industries.ToList();
            if (!string.IsNullOrWhiteSpace(IndustrySearchText))
            {
                filtered = filtered
                    .Where(i => i.Contains(IndustrySearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            FilteredIndustries = new ObservableCollection<string>(filtered);
        }

        /// <summary>
        /// // Обновление отфильтрованных компаний по тексту в фильтре
        /// </summary>
        private void UpdateFilteredCompanies()
        {
            var filtered = AvailableCompanies.ToList();
            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                filtered = filtered
                    .Where(c => c.Name?.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            FilteredCompanies = new ObservableCollection<Company>(filtered);
        }

        /// <summary>
        /// // Обновление отфильтрованных отчетов по тексту в фильтре
        /// </summary>
        private void UpdateFilteredReports()
        {
            var filtered = Reports.ToList();
            if (!string.IsNullOrWhiteSpace(ReportSearchText))
            {
                filtered = filtered
                    .Where(r => r.Contains(ReportSearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            FilteredReports = new ObservableCollection<string>(filtered);
        }

        /// <summary>
        /// // Обновление отфильтрованных новостей по тексту в фильтре
        /// </summary>
        private void UpdateFilteredNews()
        {
            var filtered = News.ToList();
            if (!string.IsNullOrWhiteSpace(NewsSearchText))
            {
                filtered = filtered
                    .Where(n => n.Contains(NewsSearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            FilteredNews = new ObservableCollection<string>(filtered);
        }

        /// <summary>
        /// // Обновление отфильтрованных компаний в зависимости от выбранных отраслей
        /// </summary>
        private void UpdateAvailableCompanies()
        {
            // Фильтрация компаний по выбранным отраслям
            var filteredCompanies = AllCompanies.ToList();
            if (SelectedIndustries.Any() && !SelectedIndustries.Contains("Все отрасли"))
            {
                filteredCompanies = filteredCompanies
                    .Where(c => c.SelectedIndustries != null && SelectedIndustries.Any(i => c.SelectedIndustries.Contains(i)))
                    .ToList();
            }
            AvailableCompanies = new ObservableCollection<Company>(filteredCompanies);
            UpdateFilteredCompanies();
            // Не отображаем выбранные компании
            var toRemove = SelectedCompanies.Where(sc => !AvailableCompanies.Contains(sc)).ToList();
            foreach (var company in toRemove)
            {
                SelectedCompanies.Remove(company);
                company.IsSelected = false;
            }
        }

        /// <summary>
        /// Обновление сводки компаний по всем фильтрам
        /// </summary>
        private void UpdateFilteredSummaries()
        {
            var selectedIndustry = SelectedIndustries.Count == 1 ? SelectedIndustries.First() : null;
            if (SelectedIndustries.Contains("Все отрасли")) selectedIndustry = null;

            // Список идентификаторов компаний
            List<int>? companyIds = SelectedCompanies.Any() ? SelectedCompanies.Select(c => c.CompanyId).ToList() : null;
            var summaries = _dbService.GetCompanySummary(selectedIndustry, companyIds, StartYear, EndYear);
            // Фильтрация по отчётам и новостям
            if (SelectedCompanies.Any())
            {
                var selectedCompanyIds = SelectedCompanies.Select(c => c.CompanyId).ToList();
                summaries = summaries
                    .Where(s => selectedCompanyIds.Contains(s.CompanyId))
                    .ToList();

                // Обновление отчётов и новостей для выбранных компаний
                var allReports = new List<string>();
                var allNews = new List<string>();
                foreach (var companyId in selectedCompanyIds)
                {
                    var reportsForCompany = _dbService.GetReports(null, null, companyId, null)
                        .Select(r => r.Title)
                        .ToList();
                    allReports.AddRange(reportsForCompany);

                    var newsForCompany = _dbService.GetNews(null, null, companyId, null)
                        .Select(n => n.Title)
                        .ToList();
                    allNews.AddRange(newsForCompany);
                }
                Reports = new ObservableCollection<string>(allReports.Distinct());
                News = new ObservableCollection<string>(allNews.Distinct());
                UpdateFilteredReports();
                UpdateFilteredNews();
                if (SelectedReports.Any()) // Фильтрация сводок по выбранным отчётам
                {
                    summaries = summaries
                        .Where(s => SelectedReports.Any(sr => s.ReportInfo?.Contains(sr, StringComparison.OrdinalIgnoreCase) == true))
                        .ToList();
                }
                if (SelectedNews.Any()) // Фильтрация сводок по выбранным новостям
                {
                    summaries = summaries
                        .Where(s => SelectedNews.Any(sn => s.NewsTitles?.Contains(sn, StringComparison.OrdinalIgnoreCase) == true))
                        .ToList();
                }
            }
            else // Если компании не выбраны, загружаем все отчёты и новости
            {
                Reports = new ObservableCollection<string>(_dbService.GetReports(null, null, null, null).Select(r => r.Title));
                News = new ObservableCollection<string>(_dbService.GetNews(null, null, null, null).Select(n => n.Title));
                UpdateFilteredReports();
                UpdateFilteredNews();
            }
            FilteredSummaries = new ObservableCollection<CompanySummary>(summaries ?? new List<CompanySummary>());
        }

        /// <summary>
        /// Очистка фильтров
        /// </summary>
        /// <param name="parameter"></param>
        private void ClearFilters(object parameter)
        {
            SelectedIndustries.Clear();
            foreach (var company in AllCompanies)
            {
                company.IsSelected = false;
            }
            SelectedCompanies.Clear();
            SelectedReports.Clear();
            SelectedNews.Clear();
            StartYear = null;
            EndYear = null;
            IndustrySearchText = string.Empty;
            CompanySearchText = string.Empty;
            ReportSearchText = string.Empty;
            NewsSearchText = string.Empty;
            UpdateFilteredIndustries();
            UpdateAvailableCompanies();
            UpdateFilteredReports();
            UpdateFilteredNews();
            UpdateFilteredSummaries();
        }

        /// <summary>
        /// Экспорт данных в CSV файл
        /// </summary>
        /// <param name="parameter"></param>
        private void ExportToCsv(object parameter)
        {
            // Проверка наличия данных для экспорта
            if (FilteredSummaries == null || FilteredSummaries.Count == 0)
            {
                MessageBox.Show("Нет данных для выгрузки.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            var saveFileDialog = new SaveFileDialog // Создание диалога для сохранения файла
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"ESG_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                DefaultExt = "csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var sb = new StringBuilder();
                sb.AppendLine("\"CompanyName\",\"Industries\",\"ReportFiles\",\"ReportInfo\",\"NewsTitles\",\"NewsInfo\",\"WebsiteUrls\",\"WebsiteInfo\",\"PeriodFrom\",\"PeriodTo\"");
                foreach (var summary in FilteredSummaries)
                {
                    var periodFrom = StartYear.HasValue ? StartYear.Value.ToString() : "N/A";
                    var periodTo = EndYear.HasValue ? EndYear.Value.ToString() : "N/A";
                    var industries = $"\"{string.Join(", ", summary.Industries?.Split(", ").Select(i => i.Replace("\"", "\"\"")) ?? new string[] { })}\"";
                    var line = $"\"{EscapeCsvField(summary.CompanyName)}\",{industries},\"{EscapeCsvField(summary.ReportFiles)}\",\"{EscapeCsvField(summary.ReportInfo)}\",\"{EscapeCsvField(summary.NewsTitles)}\",\"{EscapeCsvField(summary.NewsInfo)}\",\"{EscapeCsvField(summary.WebsiteUrls)}\",\"{EscapeCsvField(summary.WebsiteInfo)}\",\"{periodFrom}\",\"{periodTo}\"";
                    sb.AppendLine(line);
                }
                var encoding = new UTF8Encoding(true);
                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), encoding);
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveFileDialog.FileName,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Данные выгружены в файл: {saveFileDialog.FileName}\nОшибка открытия файла: {ex.Message}", "Успех с ошибкой", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        /// <summary>
        /// Экранирование полей для CSV
        /// </summary>
        /// <param name="field"></param>
        /// <returns></returns>
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "N/A";
            return $"\"{field.Replace("\"", "\"\"")}\"";
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