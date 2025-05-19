using ESG.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.Win32;
using ESG.Data;

namespace ESG.ViewModels
{
    public class ExportDataViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<string> _industries;
        private ObservableCollection<string> _filteredIndustries;
        private ObservableCollection<string> _selectedIndustries;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _availableCompanies;
        private ObservableCollection<Company> _filteredCompanies;
        private ObservableCollection<Company> _selectedCompanies;
        private ObservableCollection<string> _reports;
        private ObservableCollection<string> _filteredReports;
        private ObservableCollection<string> _selectedReports;
        private ObservableCollection<string> _news;
        private ObservableCollection<string> _filteredNews;
        private ObservableCollection<string> _selectedNews;
        private int? _startYear;
        private int? _endYear;
        private string _industrySearchText;
        private string _companySearchText;
        private string _reportSearchText;
        private string _newsSearchText;
        private ObservableCollection<CompanySummary> _filteredSummaries;

        public ExportDataViewModel()
        {
            _dbService = new DatabaseService();
            Industries = new ObservableCollection<string>(_dbService.GetIndustries().Select(i => i.IndustryName));
            FilteredIndustries = new ObservableCollection<string>(Industries);
            AllCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            AvailableCompanies = new ObservableCollection<Company>(AllCompanies);
            FilteredCompanies = new ObservableCollection<Company>(AvailableCompanies);
            SelectedIndustries = new ObservableCollection<string>();
            SelectedCompanies = new ObservableCollection<Company>();

            // Инициализация отчётов и новостей с извлечением Title
            Reports = new ObservableCollection<string>(_dbService.GetReports(null, null, null, null).Select(r => r.Title));
            FilteredReports = new ObservableCollection<string>(Reports);
            SelectedReports = new ObservableCollection<string>();

            News = new ObservableCollection<string>(_dbService.GetNews(null, null, null, null).Select(n => n.Title));
            FilteredNews = new ObservableCollection<string>(News);
            SelectedNews = new ObservableCollection<string>();

            YearsStart = new ObservableCollection<int?>
            {
                null, // Опция "Не выбрано"
            };
            for (int year = 2000; year <= DateTime.Now.Year; year++) // От 2000 до текущего года
            {
                YearsStart.Add(year);
            }

            YearsEnd = new ObservableCollection<int?>
            {
                null, // Опция "Не выбрано"
            };
            for (int year = DateTime.Now.Year; year >= 2000; year--) // От текущего года до 2000
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

            // Инициализация таблицы всеми данными при загрузке
            UpdateFilteredSummaries();

            SelectedIndustries.CollectionChanged += (s, e) => UpdateAvailableCompaniesAndClearDependencies();
            SelectedCompanies.CollectionChanged += (s, e) => UpdateFilteredSummaries();
            SelectedReports.CollectionChanged += (s, e) => UpdateFilteredSummaries();
            SelectedNews.CollectionChanged += (s, e) => UpdateFilteredSummaries();
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

        public ObservableCollection<int?> YearsStart { get; } // Для "Год с:"
        public ObservableCollection<int?> YearsEnd { get; }   // Для "Год по:"

        public ObservableCollection<string> Industries
        {
            get => _industries;
            set
            {
                _industries = value;
                OnPropertyChanged(nameof(Industries));
            }
        }

        public ObservableCollection<string> FilteredIndustries
        {
            get => _filteredIndustries;
            set
            {
                _filteredIndustries = value;
                OnPropertyChanged(nameof(FilteredIndustries));
            }
        }

        public ObservableCollection<Company> AllCompanies
        {
            get => _allCompanies;
            set
            {
                _allCompanies = value;
                OnPropertyChanged(nameof(AllCompanies));
            }
        }

        public ObservableCollection<Company> AvailableCompanies
        {
            get => _availableCompanies;
            set
            {
                _availableCompanies = value;
                OnPropertyChanged(nameof(AvailableCompanies));
            }
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

        public ObservableCollection<string> SelectedIndustries
        {
            get => _selectedIndustries;
            set
            {
                _selectedIndustries = value;
                OnPropertyChanged(nameof(SelectedIndustries));
            }
        }

        public ObservableCollection<Company> SelectedCompanies
        {
            get => _selectedCompanies;
            set
            {
                _selectedCompanies = value;
                OnPropertyChanged(nameof(SelectedCompanies));
            }
        }

        public ObservableCollection<string> Reports
        {
            get => _reports;
            set
            {
                _reports = value;
                OnPropertyChanged(nameof(Reports));
            }
        }

        public ObservableCollection<string> FilteredReports
        {
            get => _filteredReports;
            set
            {
                _filteredReports = value;
                OnPropertyChanged(nameof(FilteredReports));
            }
        }

        public ObservableCollection<string> SelectedReports
        {
            get => _selectedReports;
            set
            {
                _selectedReports = value;
                OnPropertyChanged(nameof(SelectedReports));
            }
        }

        public ObservableCollection<string> News
        {
            get => _news;
            set
            {
                _news = value;
                OnPropertyChanged(nameof(News));
            }
        }

        public ObservableCollection<string> FilteredNews
        {
            get => _filteredNews;
            set
            {
                _filteredNews = value;
                OnPropertyChanged(nameof(FilteredNews));
            }
        }

        public ObservableCollection<string> SelectedNews
        {
            get => _selectedNews;
            set
            {
                _selectedNews = value;
                OnPropertyChanged(nameof(SelectedNews));
            }
        }

        public int? StartYear
        {
            get => _startYear;
            set
            {
                _startYear = value;
                OnPropertyChanged(nameof(StartYear));
            }
        }

        public int? EndYear
        {
            get => _endYear;
            set
            {
                _endYear = value;
                OnPropertyChanged(nameof(EndYear));
            }
        }

        public string IndustrySearchText
        {
            get => _industrySearchText;
            set
            {
                _industrySearchText = value;
                OnPropertyChanged(nameof(IndustrySearchText));
            }
        }

        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                _companySearchText = value;
                OnPropertyChanged(nameof(CompanySearchText));
            }
        }

        public string ReportSearchText
        {
            get => _reportSearchText;
            set
            {
                _reportSearchText = value;
                OnPropertyChanged(nameof(ReportSearchText));
            }
        }

        public string NewsSearchText
        {
            get => _newsSearchText;
            set
            {
                _newsSearchText = value;
                OnPropertyChanged(nameof(NewsSearchText));
            }
        }

        public ObservableCollection<CompanySummary> FilteredSummaries
        {
            get => _filteredSummaries;
            set
            {
                _filteredSummaries = value;
                OnPropertyChanged(nameof(FilteredSummaries));
            }
        }

        public ICommand ExportToCsvCommand { get; }
        public ICommand ClearFiltersCommand { get; }

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

        private void UpdateAvailableCompaniesAndClearDependencies()
        {
            var filteredCompanies = AllCompanies.ToList();

            if (SelectedIndustries.Any() && !SelectedIndustries.Contains("Все отрасли"))
            {
                filteredCompanies = filteredCompanies
                    .Where(c => c.SelectedIndustries != null && SelectedIndustries.Any(i => c.SelectedIndustries.Contains(i)))
                    .ToList();
            }

            AvailableCompanies = new ObservableCollection<Company>(filteredCompanies);
            UpdateFilteredCompanies();

            var toRemove = SelectedCompanies.Where(sc => !AvailableCompanies.Contains(sc)).ToList();
            foreach (var company in toRemove)
            {
                SelectedCompanies.Remove(company);
                company.IsSelected = false;
            }
        }

        private void UpdateFilteredSummaries()
        {
            var selectedIndustry = SelectedIndustries.Count == 1 ? SelectedIndustries.First() : null;
            if (SelectedIndustries.Contains("Все отрасли")) selectedIndustry = null;

            // Получаем список идентификаторов компаний
            List<int>? companyIds = SelectedCompanies.Any() ? SelectedCompanies.Select(c => c.CompanyId).ToList() : null;

            // Вызываем GetCompanySummary один раз для всех выбранных компаний
            var summaries = _dbService.GetCompanySummary(selectedIndustry, companyIds, StartYear, EndYear);

            // Добавляем отладочный вывод для проверки данных
            System.Diagnostics.Debug.WriteLine($"Summaries count: {summaries?.Count ?? 0}");
            foreach (var summary in summaries ?? new List<CompanySummary>())
            {
                System.Diagnostics.Debug.WriteLine($"Company: {summary.CompanyName}, Industries: {summary.Industries}");
            }

            // Фильтрация по отчётам и новостям
            if (SelectedCompanies.Any())
            {
                var selectedCompanyIds = SelectedCompanies.Select(c => c.CompanyId).ToList();
                summaries = summaries
                    .Where(s => selectedCompanyIds.Contains(s.CompanyId))
                    .ToList();

                // Обновляем отчёты и новости для выбранных компаний
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

                if (SelectedReports.Any())
                {
                    summaries = summaries
                        .Where(s => SelectedReports.Any(sr => s.ReportInfo?.Contains(sr, StringComparison.OrdinalIgnoreCase) == true))
                        .ToList();
                }
                if (SelectedNews.Any())
                {
                    summaries = summaries
                        .Where(s => SelectedNews.Any(sn => s.NewsTitles?.Contains(sn, StringComparison.OrdinalIgnoreCase) == true))
                        .ToList();
                }
            }
            else
            {
                Reports = new ObservableCollection<string>(_dbService.GetReports(null, null, null, null).Select(r => r.Title));
                News = new ObservableCollection<string>(_dbService.GetNews(null, null, null, null).Select(n => n.Title));
                UpdateFilteredReports();
                UpdateFilteredNews();
            }

            FilteredSummaries = new ObservableCollection<CompanySummary>(summaries ?? new List<CompanySummary>());
        }

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
            UpdateAvailableCompaniesAndClearDependencies();
            UpdateFilteredReports();
            UpdateFilteredNews();
            UpdateFilteredSummaries();
        }

        private void ExportToCsv(object parameter)
        {
            if (FilteredSummaries == null || FilteredSummaries.Count == 0)
            {
                MessageBox.Show("Нет данных для выгрузки.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var saveFileDialog = new SaveFileDialog
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

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "N/A";
            return $"\"{field.Replace("\"", "\"\"")}\"";
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}