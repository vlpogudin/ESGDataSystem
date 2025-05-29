using ESG.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using ESG.Data;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

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
        /// Команда экспорта данных в Excel
        /// </summary>
        public ICommand ExportToExcelCommand { get; }

        /// <summary>
        /// Команда очистки всех фильтров
        /// </summary>
        public ICommand ClearFiltersCommand { get; }

        #endregion

        #region New Properties

        private bool _isExporting;
        public bool IsExporting
        {
            get => _isExporting;
            set
            {
                _isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
            }
        }

        private string _exportProgressMessage;
        public string ExportProgressMessage
        {
            get => _exportProgressMessage;
            set
            {
                _exportProgressMessage = value;
                OnPropertyChanged(nameof(ExportProgressMessage));
            }
        }

        private double _exportProgress;
        public double ExportProgress
        {
            get => _exportProgress;
            set
            {
                _exportProgress = value;
                OnPropertyChanged(nameof(ExportProgress));
            }
        }

        private bool _isIndeterminate;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }

        #endregion

        #region New Commands

        public ICommand ExportToPdfCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация VM
        /// </summary>
        public ExportDataViewModel()
        {
            _dbService = new DatabaseService(); // Инициализация сервиса базы данных

            // Set EPPlus license context
            OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

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
            ExportToExcelCommand = new RelayCommand(ExportToExcel);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            ExportToPdfCommand = new RelayCommand(ExportToPdf);

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

                // Создаем словарь для хранения отчетов по компаниям
                var reportsByCompany = new Dictionary<int, List<string>>();
                var newsByCompany = new Dictionary<int, List<string>>();

                // Заполняем словари данными для каждой компании
                foreach (var companyId in selectedCompanyIds)
                {
                    var reportsForCompany = _dbService.GetReports(null, null, companyId, null)
                        .Select(r => r.Title)
                        .ToList();
                    reportsByCompany[companyId] = reportsForCompany;

                    var newsForCompany = _dbService.GetNews(null, null, companyId, null)
                        .Select(n => n.Title)
                        .ToList();
                    newsByCompany[companyId] = newsForCompany;
                }

                // Обновляем коллекции Reports и News только для выбранных компаний
                Reports = new ObservableCollection<string>(reportsByCompany.Values.SelectMany(r => r).Distinct());
                News = new ObservableCollection<string>(newsByCompany.Values.SelectMany(n => n).Distinct());

                // Фильтруем сводки по выбранным отчетам
                if (SelectedReports.Any())
                {
                    summaries = summaries
                        .Where(s => reportsByCompany.ContainsKey(s.CompanyId) && 
                                   reportsByCompany[s.CompanyId].Any(r => SelectedReports.Contains(r)))
                        .ToList();
                }

                // Фильтруем сводки по выбранным новостям
                if (SelectedNews.Any())
                {
                    summaries = summaries
                        .Where(s => newsByCompany.ContainsKey(s.CompanyId) && 
                                   newsByCompany[s.CompanyId].Any(n => SelectedNews.Contains(n)))
                        .ToList();
                }

                UpdateFilteredReports();
                UpdateFilteredNews();
            }
            else
            {
                // Если компании не выбраны, очищаем коллекции отчетов и новостей
                Reports = new ObservableCollection<string>();
                News = new ObservableCollection<string>();
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
        /// Экспорт данных в Excel файл
        /// </summary>
        /// <param name="parameter"></param>
        private void ExportToExcel(object parameter)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx",
                FileName = $"ESG_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                DefaultExt = "xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Создаем основную папку выгрузки
                    string exportDir = Path.GetDirectoryName(saveFileDialog.FileName);
                    string exportFolder = Path.Combine(exportDir, $"Выгрузка_{DateTime.Now:yyyyMMdd_HHmmss}");
                    Directory.CreateDirectory(exportFolder);

                    // Создаем папку для отчетов внутри папки выгрузки
                    string reportsFolder = Path.Combine(exportFolder, "Reports");
                    Directory.CreateDirectory(reportsFolder);

                    // Путь для сохранения Excel файла
                    string excelFilePath = Path.Combine(exportFolder, Path.GetFileName(saveFileDialog.FileName));

                    using (var package = new ExcelPackage())
                    {
                        // Основной лист с данными
                        var worksheet = package.Workbook.Worksheets.Add("Основные данные");
                        
                        // Заголовки
                        worksheet.Cells[1, 1].Value = "Компания";
                        worksheet.Cells[1, 2].Value = "Отрасли";
                        worksheet.Cells[1, 3].Value = "Отчеты";
                        worksheet.Cells[1, 4].Value = "Информация об отчетах";
                        worksheet.Cells[1, 5].Value = "Новости";
                        worksheet.Cells[1, 6].Value = "Информация о новостях";
                        worksheet.Cells[1, 7].Value = "Веб-сайты";
                        worksheet.Cells[1, 8].Value = "Информация о веб-сайтах";
                        worksheet.Cells[1, 9].Value = "Период с";
                        worksheet.Cells[1, 10].Value = "Период по";

                        // Стилизация заголовков
                        using (var range = worksheet.Cells[1, 1, 1, 10])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }

                        // Заполнение данных
                        int row = 2;
                        foreach (var summary in FilteredSummaries)
                        {
                            worksheet.Cells[row, 1].Value = summary.CompanyName;
                            worksheet.Cells[row, 2].Value = string.Join(", ", summary.Industries?.Split(", ").Distinct() ?? new string[] { });
                            
                            // Форматируем отчеты и их информацию с разделителем ;
                            var reports = summary.ReportFiles?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
                            var reportInfo = summary.ReportInfo?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
                            worksheet.Cells[row, 3].Value = string.Join(";\n", reports);
                            worksheet.Cells[row, 4].Value = string.Join(";\n", reportInfo);

                            // Форматируем новости и их информацию с разделителем ;
                            var news = summary.NewsTitles?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
                            var newsInfo = summary.NewsInfo?.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries) ?? new string[] { };
                            worksheet.Cells[row, 5].Value = string.Join(";\n", news);
                            worksheet.Cells[row, 6].Value = string.Join(";\n", newsInfo);

                            worksheet.Cells[row, 7].Value = summary.WebsiteUrls;
                            worksheet.Cells[row, 8].Value = summary.WebsiteInfo;
                            worksheet.Cells[row, 9].Value = StartYear?.ToString() ?? "N/A";
                            worksheet.Cells[row, 10].Value = EndYear?.ToString() ?? "N/A";

                            // Добавляем границы для каждой строки
                            using (var range = worksheet.Cells[row, 1, row, 10])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.WrapText = true; // Включаем перенос текста
                            }
                            row++;
                        }

                        // Устанавливаем ширину столбцов для основного листа
                        worksheet.Column(1).Width = 40; // Компания
                        worksheet.Column(2).Width = 35; // Отрасли
                        worksheet.Column(3).Width = 60; // Отчеты
                        worksheet.Column(4).Width = 60; // Информация об отчетах
                        worksheet.Column(5).Width = 60; // Новости
                        worksheet.Column(6).Width = 60; // Информация о новостях
                        worksheet.Column(7).Width = 60; // Веб-сайты
                        worksheet.Column(8).Width = 60; // Информация о веб-сайтах
                        worksheet.Column(9).Width = 15; // Период с
                        worksheet.Column(10).Width = 15; // Период по

                        // Устанавливаем высоту строк для основного листа
                        for (int i = 2; i < row; i++)
                        {
                            worksheet.Row(i).Height = 60; // Уменьшаем высоту строк
                        }

                        // Добавляем фильтры для заголовков
                        worksheet.Cells[1, 1, 1, 10].AutoFilter = true;

                        // Лист с отчетами
                        var reportsSheet = package.Workbook.Worksheets.Add("Отчеты");
                        reportsSheet.Cells[1, 1].Value = "Компания";
                        reportsSheet.Cells[1, 2].Value = "Название отчета";
                        reportsSheet.Cells[1, 3].Value = "Год";
                        reportsSheet.Cells[1, 4].Value = "Язык";
                        reportsSheet.Cells[1, 5].Value = "Путь к файлу";

                        // Устанавливаем ширину столбцов для листа отчетов
                        reportsSheet.Column(1).Width = 40; // Компания
                        reportsSheet.Column(2).Width = 60; // Название отчета
                        reportsSheet.Column(3).Width = 15; // Год
                        reportsSheet.Column(4).Width = 20; // Язык
                        reportsSheet.Column(5).Width = 60; // Путь к файлу

                        // Добавляем фильтры для заголовков отчетов
                        reportsSheet.Cells[1, 1, 1, 5].AutoFilter = true;

                        int reportRow = 2;
                        var processedReports = new HashSet<int>(); // Для отслеживания уже добавленных отчетов

                        foreach (var summary in FilteredSummaries)
                        {
                            // Получаем отчеты только для текущей компании
                            var companyReports = _dbService.GetReports(null, null, summary.CompanyId, null)
                                .Where(r => r.CompanyId == summary.CompanyId)
                                .ToList();

                            if (companyReports.Any())
                            {
                                string sanitizedCompanyName = SanitizeFileName(summary.CompanyName);
                                string companyFolder = Path.Combine(reportsFolder, sanitizedCompanyName);
                                Directory.CreateDirectory(companyFolder);

                                foreach (var report in companyReports)
                                {
                                    // Проверяем, не добавляли ли мы уже этот отчет
                                    if (!processedReports.Contains(report.ReportId))
                                    {
                                        processedReports.Add(report.ReportId);

                                        reportsSheet.Cells[reportRow, 1].Value = summary.CompanyName;
                                        reportsSheet.Cells[reportRow, 2].Value = report.Title;
                                        reportsSheet.Cells[reportRow, 3].Value = report.Year?.ToString() ?? "N/A";
                                        reportsSheet.Cells[reportRow, 4].Value = report.Language;
                                        reportsSheet.Cells[reportRow, 5].Value = report.FilePath;

                                        // Копируем файл отчета в папку компании
                                        if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
                                        {
                                            string fileName = Path.GetFileName(report.FilePath);
                                            string sanitizedFileName = SanitizeFileName(fileName);
                                            string destPath = Path.Combine(companyFolder, sanitizedFileName);
                                            File.Copy(report.FilePath, destPath, true);
                                        }

                                        reportRow++;
                                    }
                                }
                            }
                        }

                        // Стилизация листа отчетов
                        using (var range = reportsSheet.Cells[1, 1, 1, 5])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }

                        // Добавляем границы и перенос текста для всех строк в листе отчетов
                        for (int i = 2; i < reportRow; i++)
                        {
                            using (var range = reportsSheet.Cells[i, 1, i, 5])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                range.Style.WrapText = true;
                            }
                            reportsSheet.Row(i).Height = 30;
                        }

                        // Лист с веб-сайтами
                        var websitesSheet = package.Workbook.Worksheets.Add("Веб-сайты");
                        websitesSheet.Cells[1, 1].Value = "Компания";
                        websitesSheet.Cells[1, 2].Value = "URL";
                        websitesSheet.Cells[1, 3].Value = "Описание";

                        // Устанавливаем ширину столбцов для листа веб-сайтов
                        websitesSheet.Column(1).Width = 40; // Компания
                        websitesSheet.Column(2).Width = 60; // URL
                        websitesSheet.Column(3).Width = 60; // Описание

                        // Добавляем фильтры для заголовков веб-сайтов
                        websitesSheet.Cells[1, 1, 1, 3].AutoFilter = true;

                        int websiteRow = 2;
                        var processedWebsites = new HashSet<string>(); // Для отслеживания уже добавленных веб-сайтов

                        foreach (var summary in FilteredSummaries)
                        {
                            if (!string.IsNullOrEmpty(summary.WebsiteUrls))
                            {
                                // Разбиваем строки по символу ";"
                                var urls = summary.WebsiteUrls.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(u => u.Trim())
                                    .Where(u => !string.IsNullOrWhiteSpace(u))
                                    .ToList();
                                
                                var infos = summary.WebsiteInfo?.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(i => i.Trim())
                                    .Where(i => !string.IsNullOrWhiteSpace(i))
                                    .ToList() ?? new List<string>();

                                for (int i = 0; i < urls.Count; i++)
                                {
                                    string url = urls[i];
                                    // Проверяем, не добавляли ли мы уже этот веб-сайт
                                    if (!processedWebsites.Contains(url))
                                    {
                                        processedWebsites.Add(url);

                                        websitesSheet.Cells[websiteRow, 1].Value = summary.CompanyName;
                                        websitesSheet.Cells[websiteRow, 2].Value = url;
                                        websitesSheet.Cells[websiteRow, 3].Value = i < infos.Count ? infos[i] : "";

                                        // Добавляем границы и перенос текста для каждой строки
                                        using (var range = websitesSheet.Cells[websiteRow, 1, websiteRow, 3])
                                        {
                                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                            range.Style.WrapText = true;
                                        }
                                        websiteRow++;
                                    }
                                }
                            }
                        }

                        // Стилизация листа веб-сайтов
                        using (var range = websitesSheet.Cells[1, 1, 1, 3])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightYellow);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }

                        // Устанавливаем высоту строк для листа веб-сайтов
                        for (int i = 2; i < websiteRow; i++)
                        {
                            websitesSheet.Row(i).Height = 30;
                        }

                        // Лист с новостями
                        var newsSheet = package.Workbook.Worksheets.Add("Новости");
                        newsSheet.Cells[1, 1].Value = "Компания";
                        newsSheet.Cells[1, 2].Value = "Заголовок";
                        newsSheet.Cells[1, 3].Value = "Информация";

                        // Устанавливаем ширину столбцов для листа новостей
                        newsSheet.Column(1).Width = 40; // Компания
                        newsSheet.Column(2).Width = 60; // Заголовок
                        newsSheet.Column(3).Width = 60; // Информация

                        // Добавляем фильтры для заголовков новостей
                        newsSheet.Cells[1, 1, 1, 3].AutoFilter = true;

                        int newsRow = 2;
                        var processedNews = new HashSet<string>(); // Для отслеживания уже добавленных новостей

                        foreach (var summary in FilteredSummaries)
                        {
                            if (!string.IsNullOrEmpty(summary.NewsTitles))
                            {
                                // Разбиваем строки по символу ";"
                                var titles = summary.NewsTitles.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(t => t.Trim())
                                    .Where(t => !string.IsNullOrWhiteSpace(t))
                                    .ToList();
                                
                                var infos = summary.NewsInfo?.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                    .Select(i => i.Trim())
                                    .Where(i => !string.IsNullOrWhiteSpace(i))
                                    .ToList() ?? new List<string>();

                                for (int i = 0; i < titles.Count; i++)
                                {
                                    string title = titles[i];
                                    // Проверяем, не добавляли ли мы уже эту новость
                                    if (!processedNews.Contains(title))
                                    {
                                        processedNews.Add(title);

                                        newsSheet.Cells[newsRow, 1].Value = summary.CompanyName;
                                        newsSheet.Cells[newsRow, 2].Value = title;
                                        newsSheet.Cells[newsRow, 3].Value = i < infos.Count ? infos[i] : "";

                                        // Добавляем границы и перенос текста для каждой строки
                                        using (var range = newsSheet.Cells[newsRow, 1, newsRow, 3])
                                        {
                                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                                            range.Style.WrapText = true;
                                        }
                                        newsRow++;
                                    }
                                }
                            }
                        }

                        // Стилизация листа новостей
                        using (var range = newsSheet.Cells[1, 1, 1, 3])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightPink);
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }

                        // Устанавливаем высоту строк для листа новостей
                        for (int i = 2; i < newsRow; i++)
                        {
                            newsSheet.Row(i).Height = 30;
                        }

                        // Сводный лист
                        var summarySheet = package.Workbook.Worksheets.Add("Сводная информация");
                        
                        // Заголовок сводки
                        summarySheet.Cells[1, 1].Value = "Сводная информация по ESG данным";
                        summarySheet.Cells[1, 1, 1, 2].Merge = true;
                        using (var range = summarySheet.Cells[1, 1, 1, 2])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Font.Size = 14;
                            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightBlue);
                        }

                        // Статистика
                        summarySheet.Cells[3, 1].Value = "Всего компаний:";
                        summarySheet.Cells[3, 2].Value = FilteredSummaries.Count;
                        summarySheet.Cells[4, 1].Value = "Период:";
                        summarySheet.Cells[4, 2].Value = $"{StartYear?.ToString() ?? "N/A"} - {EndYear?.ToString() ?? "N/A"}";
                        summarySheet.Cells[5, 1].Value = "Всего отраслей:";
                        summarySheet.Cells[5, 2].Value = SelectedIndustries.Count;
                        summarySheet.Cells[6, 1].Value = "Всего веб-сайтов:";
                        summarySheet.Cells[6, 2].Value = websiteRow - 2; // Используем реальное количество веб-сайтов
                        summarySheet.Cells[7, 1].Value = "Всего новостей:";
                        summarySheet.Cells[7, 2].Value = newsRow - 2; // Используем реальное количество новостей
                        summarySheet.Cells[8, 1].Value = "Всего отчетов:";
                        summarySheet.Cells[8, 2].Value = reportRow - 2; // Используем реальное количество отчетов

                        // Стилизация статистики
                        using (var range = summarySheet.Cells[3, 1, 8, 2])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.White);
                        }

                        // Устанавливаем ширину столбцов для сводного листа
                        summarySheet.Column(1).Width = 30;
                        summarySheet.Column(2).Width = 30;

                        // Устанавливаем высоту строк для сводного листа
                        for (int i = 1; i <= 8; i++)
                        {
                            summarySheet.Row(i).Height = 30;
                        }

                        // Устанавливаем порядок листов
                        package.Workbook.Worksheets.MoveToStart("Основные данные");
                        package.Workbook.Worksheets.MoveAfter("Отчеты", "Основные данные");
                        package.Workbook.Worksheets.MoveAfter("Веб-сайты", "Отчеты");
                        package.Workbook.Worksheets.MoveAfter("Новости", "Веб-сайты");
                        package.Workbook.Worksheets.MoveAfter("Сводная информация", "Новости");

                        // Сохранение файла в папку выгрузки
                        var fileInfo = new FileInfo(excelFilePath);
                        package.SaveAs(fileInfo);

                        // Проверяем, что файл действительно создался
                        if (!File.Exists(excelFilePath))
                        {
                            throw new Exception("Не удалось создать Excel файл");
                        }

                        // Копируем отчеты
                        foreach (var summary in FilteredSummaries)
                        {
                            var companyReports = _dbService.GetReports(null, null, summary.CompanyId, null)
                                .Where(r => r.CompanyId == summary.CompanyId)
                                .ToList();

                            if (companyReports.Any())
                            {
                                string sanitizedCompanyName = SanitizeFileName(summary.CompanyName);
                                string companyFolder = Path.Combine(reportsFolder, sanitizedCompanyName);
                                Directory.CreateDirectory(companyFolder);

                                foreach (var report in companyReports)
                                {
                                    if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
                                    {
                                        try
                                        {
                                            string fileName = Path.GetFileName(report.FilePath);
                                            string sanitizedFileName = SanitizeFileName(fileName);
                                            string destPath = Path.Combine(companyFolder, sanitizedFileName);
                                            File.Copy(report.FilePath, destPath, true);
                                        }
                                        catch (Exception ex)
                                        {
                                            Debug.WriteLine($"Ошибка при копировании файла {report.FilePath}: {ex.Message}");
                                        }
                                    }
                                }
                            }
                        }

                        // Проверяем, что папка с отчетами не пустая
                        if (!Directory.GetFiles(reportsFolder, "*.*", SearchOption.AllDirectories).Any())
                        {
                            Debug.WriteLine("Папка с отчетами пуста");
                        }
                    }

                    MessageBox.Show($"Данные успешно выгружены!\nExcel файл и отчеты находятся в папке: {exportFolder}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выгрузке данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
        /// Экспорт данных в PDF файл
        /// </summary>
        private void ExportToPdf(object parameter)
        {
            if (FilteredSummaries == null || FilteredSummaries.Count == 0)
            {
                MessageBox.Show("Нет данных для выгрузки.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "PDF files (*.pdf)|*.pdf",
                FileName = $"ESG_Summary_{DateTime.Now:yyyyMMdd_HHmmss}.pdf",
                DefaultExt = "pdf"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Register encoding provider
                    System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                    // Create base font with Unicode support
                    string fontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
                    BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

                    using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        Document document = new Document(PageSize.A4.Rotate(), 25, 25, 30, 30);
                        PdfWriter writer = PdfWriter.GetInstance(document, fs);
                        
                        document.Open();

                        // Add title
                        iTextSharp.text.Font titleFont = new iTextSharp.text.Font(baseFont, 16f, iTextSharp.text.Font.BOLD);
                        Paragraph title = new Paragraph("Сводка ESG данных", titleFont);
                        title.Alignment = Element.ALIGN_CENTER;
                        title.SpacingAfter = 20f;
                        document.Add(title);

                        // Create table
                        PdfPTable table = new PdfPTable(8); // 8 columns
                        table.WidthPercentage = 100;

                        // Устанавливаем ширину столбцов
                        float[] columnWidths = new float[] { 15f, 15f, 15f, 15f, 15f, 25f, 15f, 15f }; // Изменяем ширину столбцов
                        table.SetWidths(columnWidths);

                        // Add headers
                        iTextSharp.text.Font headerFont = new iTextSharp.text.Font(baseFont, 10f, iTextSharp.text.Font.BOLD);
                        string[] headers = { "Компания", "Отрасли", "Отчеты", "Информация об отчетах", "Новости", "Информация о новостях", "Веб-сайты", "Информация о веб-сайтах" };
                        foreach (string header in headers)
                        {
                            PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                            cell.Padding = 5f;
                            table.AddCell(cell);
                        }

                        // Add data
                        iTextSharp.text.Font dataFont = new iTextSharp.text.Font(baseFont, 9f);
                        foreach (var summary in FilteredSummaries)
                        {
                            // Format URLs to remove semicolons and extra spaces
                            string formattedUrls = summary.WebsiteUrls?.Replace(" ; ", "\n")?.Replace(";", "")?.Trim() ?? "N/A";
                            string formattedInfo = summary.WebsiteInfo?.Replace(" ; ", "\n")?.Replace(";", "")?.Trim() ?? "N/A";

                            // Create cells with proper encoding
                            table.AddCell(new PdfPCell(new Phrase(summary.CompanyName ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(summary.Industries ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(summary.ReportFiles ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(summary.ReportInfo ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(summary.NewsTitles ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(summary.NewsInfo ?? "N/A", dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(formattedUrls, dataFont)) { Padding = 5f });
                            table.AddCell(new PdfPCell(new Phrase(formattedInfo, dataFont)) { Padding = 5f });
                        }

                        document.Add(table);

                        // Add footer with date
                        iTextSharp.text.Font footerFont = new iTextSharp.text.Font(baseFont, 8f);
                        Paragraph footer = new Paragraph($"Создано: {DateTime.Now:yyyy-MM-dd HH:mm:ss}", footerFont);
                        footer.Alignment = Element.ALIGN_RIGHT;
                        document.Add(footer);

                        document.Close();
                    }

                    MessageBox.Show($"Данные успешно выгружены в PDF файл: {saveFileDialog.FileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при выгрузке данных в PDF: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return fileName;
            
            // Заменяем недопустимые символы на подчеркивание
            char[] invalidChars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c, '_');
            }
            
            // Удаляем кавычки
            fileName = fileName.Replace("\"", "").Replace("'", "");
            
            // Удаляем символы | и /
            fileName = fileName.Replace("|", "_").Replace("/", "_");
            
            return fileName;
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