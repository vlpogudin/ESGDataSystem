using ESG.Data;
using ESG.Models;
using ESG.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ESG.Utilities;

namespace ESG.ViewModels
{
    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private Report _selectedReport;
        private ObservableCollection<Report> _allReports;
        private ObservableCollection<Report> _filteredReports;
        private ObservableCollection<Report> _availableReports;
        private ObservableCollection<Report> _selectedReports;
        private ObservableCollection<Report> _filteredForList;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompaniesForList;
        private ObservableCollection<Company> _selectedCompanies;
        private string _reportSearchText;
        private string _companySearchText;

        public ReportsViewModel()
        {
            _dbService = new DatabaseService();
            _allReports = new ObservableCollection<Report>(_dbService.GetReports(null, null, null, null));
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());

            foreach (var report in _allReports)
            {
                report.CompanyName = _dbService.GetCompanyNameById(report.CompanyId) ?? "Неизвестная компания";
            }

            AvailableReports = new ObservableCollection<Report>(_allReports);
            FilteredForList = new ObservableCollection<Report>(AvailableReports);
            FilteredReports = new ObservableCollection<Report>(AvailableReports);
            SelectedReports = new ObservableCollection<Report>();
            FilteredCompaniesForList = new ObservableCollection<Company>(_allCompanies);
            SelectedCompanies = new ObservableCollection<Company>();

            AddCommand = new RelayCommand(_ => AddReport(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditReport(), _ => CanPerformCrud && SelectedReport != null);
            DeleteCommand = new RelayCommand(_ => DeleteReport(), _ => CanPerformCrud && SelectedReport != null);
            OpenFileCommand = new RelayCommand(OpenFile);
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
            ClearCompanySelectionCommand = new RelayCommand(_ => ClearCompanySelection());
            ClearReportSelectionCommand = new RelayCommand(_ => ClearReportSelection());
            ReportSearchText = string.Empty;
            CompanySearchText = string.Empty;

            SelectedCompanies.CollectionChanged += (s, e) => UpdateFilteredReports();
            SelectedReports.CollectionChanged += (s, e) => UpdateTableReports();
        }

        public ObservableCollection<Report> FilteredReports
        {
            get => _filteredReports;
            set
            {
                _filteredReports = value;
                OnPropertyChanged(nameof(FilteredReports));
            }
        }

        public ObservableCollection<Report> AvailableReports
        {
            get => _availableReports;
            set
            {
                _availableReports = value;
                OnPropertyChanged(nameof(AvailableReports));
            }
        }

        public ObservableCollection<Report> FilteredForList
        {
            get => _filteredForList;
            set
            {
                _filteredForList = value;
                OnPropertyChanged(nameof(FilteredForList));
            }
        }

        public ObservableCollection<Report> SelectedReports
        {
            get => _selectedReports;
            set
            {
                _selectedReports = value;
                OnPropertyChanged(nameof(SelectedReports));
            }
        }

        public ObservableCollection<Company> FilteredCompaniesForList
        {
            get => _filteredCompaniesForList;
            set
            {
                _filteredCompaniesForList = value;
                OnPropertyChanged(nameof(FilteredCompaniesForList));
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

        public Report SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                OnPropertyChanged(nameof(SelectedReport));
            }
        }

        public string ReportSearchText
        {
            get => _reportSearchText;
            set
            {
                _reportSearchText = value;
                OnPropertyChanged(nameof(ReportSearchText));
                UpdateFilteredForList();
                UpdateTableReports();
            }
        }

        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                _companySearchText = value;
                OnPropertyChanged(nameof(CompanySearchText));
                UpdateFilteredCompaniesForList();
            }
        }

        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand ExportToCsvCommand { get; }
        public ICommand ClearCompanySelectionCommand { get; }
        public ICommand ClearReportSelectionCommand { get; }

        public void UpdateFilteredCompaniesForList()
        {
            var filtered = _allCompanies.ToList();
            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                filtered = filtered
                    .Where(c => c.Name?.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            FilteredCompaniesForList = new ObservableCollection<Company>(filtered);

            var toRemove = SelectedCompanies.Where(sc => !_allCompanies.Contains(sc)).ToList();
            foreach (var company in toRemove)
            {
                SelectedCompanies.Remove(company);
                company.IsSelected = false;
            }
            UpdateFilteredForList();
        }

        public void UpdateFilteredForList()
        {
            var filtered = AvailableReports.ToList();

            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(r => SelectedCompanies.Any(c => c.CompanyId == r.CompanyId)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(ReportSearchText))
            {
                filtered = filtered
                    .Where(r => r.Title?.Contains(ReportSearchText, StringComparison.OrdinalIgnoreCase) == true ||
                                r.CompanyName?.Contains(ReportSearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }

            FilteredForList = new ObservableCollection<Report>(filtered);

            var toRemove = SelectedReports.Where(sr => !filtered.Contains(sr)).ToList();
            foreach (var report in toRemove)
            {
                SelectedReports.Remove(report);
                report.IsSelected = false;
            }
        }

        private void UpdateTableReports()
        {
            var filtered = AvailableReports.AsEnumerable();

            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(r => SelectedCompanies.Any(c => c.CompanyId == r.CompanyId));
            }

            if (SelectedReports.Any())
            {
                filtered = filtered.Where(r => SelectedReports.Contains(r));
            }

            if (!string.IsNullOrWhiteSpace(ReportSearchText))
            {
                filtered = filtered
                    .Where(r => r.Title.ToLower().Contains(ReportSearchText.ToLower()) ||
                                r.CompanyName.ToLower().Contains(ReportSearchText.ToLower()));
            }

            FilteredReports = new ObservableCollection<Report>(filtered.ToList());
            OnPropertyChanged(nameof(FilteredReports));
        }

        public void UpdateFilteredReports()
        {
            UpdateFilteredCompaniesForList();
            UpdateFilteredForList();
            UpdateTableReports();
        }

        private void AddReport()
        {
            var window = new AddReportWindow(new Report());
            if (window.ShowDialog() == true)
            {
                var newReport = window.Report;
                _dbService.AddReport(newReport);

                _allReports = new ObservableCollection<Report>(_dbService.GetReports(null, null, null, null));
                foreach (var report in _allReports)
                {
                    report.CompanyName = _dbService.GetCompanyNameById(report.CompanyId) ?? "Неизвестная компания";
                }
                AvailableReports = new ObservableCollection<Report>(_allReports);
                UpdateFilteredReports();
                SelectedReport = _allReports.FirstOrDefault(r => r.ReportId == newReport.ReportId);
            }
        }

        private void EditReport()
        {
            var window = new EditReportWindow(SelectedReport);
            if (window.ShowDialog() == true)
            {
                _dbService.UpdateReport(SelectedReport);
                _allReports = new ObservableCollection<Report>(_dbService.GetReports(null, null, null, null));
                foreach (var report in _allReports)
                {
                    report.CompanyName = _dbService.GetCompanyNameById(report.CompanyId) ?? "Неизвестная компания";
                }
                AvailableReports = new ObservableCollection<Report>(_allReports);
                UpdateFilteredReports();
            }
        }

        private void DeleteReport()
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить отчёт '{SelectedReport.Title}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _dbService.DeleteReport(SelectedReport.ReportId);
                _allReports.Remove(SelectedReport);
                AvailableReports = new ObservableCollection<Report>(_allReports);
                UpdateFilteredReports();
            }
        }

        private void OpenFile(object parameter)
        {
            if (parameter is string filePath)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToCsv()
        {
            if (SelectedReports == null || !SelectedReports.Any())
            {
                MessageBox.Show("Выберите хотя бы один отчёт для выгрузки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"ReportsExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("ID,Компания,Название отчёта,Год,Язык,Файл");

                    foreach (var report in SelectedReports)
                    {
                        var id = report.ReportId.ToString();
                        var companyName = $"\"{report.CompanyName?.Replace("\"", "\"\"") ?? ""}\"";
                        var title = $"\"{report.Title?.Replace("\"", "\"\"") ?? ""}\"";
                        var year = report.Year?.ToString() ?? "";
                        var language = $"\"{report.Language?.Replace("\"", "\"\"") ?? ""}\"";
                        var filePath = $"\"{report.FilePath?.Replace("\"", "\"\"") ?? ""}\"";
                        csvBuilder.AppendLine($"{id},{companyName},{title},{year},{language},{filePath}");
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

        private void ClearCompanySelection()
        {
            foreach (var company in SelectedCompanies.ToList())
            {
                company.IsSelected = false;
                SelectedCompanies.Remove(company);
            }
            UpdateFilteredReports();
        }

        private void ClearReportSelection()
        {
            foreach (var report in SelectedReports.ToList())
            {
                report.IsSelected = false;
                SelectedReports.Remove(report);
            }
            UpdateTableReports();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}