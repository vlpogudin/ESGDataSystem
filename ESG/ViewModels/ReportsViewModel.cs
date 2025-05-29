using ESG.Data;
using ESG.Models;
using ESG.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using ESG.Utilities;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

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
            
            // Получаем отчеты и компании из базы данных
            var reports = _dbService.GetReports(null, null, null, null);
            var companies = _dbService.GetCompanies();

            // Инициализируем коллекции
            _allReports = new ObservableCollection<Report>();
            _allCompanies = new ObservableCollection<Company>(companies);

            // Добавляем отчеты по одному, устанавливая имена компаний
            foreach (var report in reports)
            {
                report.CompanyName = _dbService.GetCompanyNameById(report.CompanyId) ?? "Неизвестная компания";
                _allReports.Add(report);
            }

            // Инициализируем остальные коллекции
            AvailableReports = new ObservableCollection<Report>(_allReports);
            FilteredForList = new ObservableCollection<Report>(_allReports);
            FilteredReports = new ObservableCollection<Report>(_allReports);
            SelectedReports = new ObservableCollection<Report>();
            FilteredCompaniesForList = new ObservableCollection<Company>(_allCompanies);
            SelectedCompanies = new ObservableCollection<Company>();

            // Инициализируем команды
            AddCommand = new RelayCommand(_ => AddReport(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditReport(), _ => CanPerformCrud && SelectedReport != null);
            DeleteCommand = new RelayCommand(_ => DeleteReport(), _ => CanPerformCrud && SelectedReport != null);
            OpenFileCommand = new RelayCommand(OpenFile);
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
            ClearCompanySelectionCommand = new RelayCommand(_ => ClearCompanySelection());
            ClearReportSelectionCommand = new RelayCommand(_ => ClearReportSelection());

            // Инициализируем тексты поиска
            ReportSearchText = string.Empty;
            CompanySearchText = string.Empty;

            // Подписываемся на события изменения коллекций
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
            // Создаем новый список для отфильтрованных отчетов
            var filtered = new List<Report>();

            // Добавляем отчеты из AvailableReports, которые соответствуют фильтрам
            foreach (var report in AvailableReports)
            {
                bool matchesCompanyFilter = !SelectedCompanies.Any() || 
                    SelectedCompanies.Any(c => c.CompanyId == report.CompanyId);
                
                bool matchesReportFilter = !SelectedReports.Any() || 
                    SelectedReports.Contains(report);
                
                bool matchesSearchFilter = string.IsNullOrWhiteSpace(ReportSearchText) ||
                    (report.Title?.ToLower().Contains(ReportSearchText.ToLower()) == true) ||
                    (report.CompanyName?.ToLower().Contains(ReportSearchText.ToLower()) == true);

                // Проверяем, не добавлен ли уже этот отчет в отфильтрованный список
                bool isAlreadyAdded = filtered.Any(r => r.ReportId == report.ReportId);

                if (matchesCompanyFilter && matchesReportFilter && matchesSearchFilter && !isAlreadyAdded)
                {
                    filtered.Add(report);
                }
            }

            // Обновляем FilteredReports
            FilteredReports = new ObservableCollection<Report>(filtered);
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

                // Получаем обновленный список отчетов
                var updatedReports = _dbService.GetReports(null, null, null, null);
                
                // Очищаем и обновляем все коллекции
                _allReports.Clear();
                foreach (var report in updatedReports)
                {
                    report.CompanyName = _dbService.GetCompanyNameById(report.CompanyId) ?? "Неизвестная компания";
                    _allReports.Add(report);
                }

                // Обновляем AvailableReports
                AvailableReports.Clear();
                foreach (var report in _allReports)
                {
                    AvailableReports.Add(report);
                }

                // Обновляем фильтры
                UpdateFilteredReports();

                // Выбираем новый отчет
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
                    // Create a folder for report files in the same directory as the CSV file
                    string exportDir = Path.GetDirectoryName(saveFileDialog.FileName);
                    string reportsFolder = Path.Combine(exportDir, "Reports");
                    Directory.CreateDirectory(reportsFolder);

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

                        // Copy report file if it exists
                        if (!string.IsNullOrEmpty(report.FilePath) && File.Exists(report.FilePath))
                        {
                            string fileName = Path.GetFileName(report.FilePath);
                            string newFilePath = Path.Combine(reportsFolder, fileName);
                            File.Copy(report.FilePath, newFilePath, true);
                        }

                        csvBuilder.AppendLine($"{id},{companyName},{title},{year},{language},{filePath}");
                    }

                    System.IO.File.WriteAllText(saveFileDialog.FileName, csvBuilder.ToString(), Encoding.UTF8);
                    MessageBox.Show($"Данные успешно выгружены в CSV!\nФайлы отчётов скопированы в папку: {reportsFolder}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
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

        public void ExportChangeLog()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = "Сохранить журнал изменений",
                    FileName = $"Отчеты_Журнал_изменений_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var logs = _dbService.GetChangeLog("Reports");
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Журнал изменений");

                        // Заголовки
                        worksheet.Cells[1, 1].Value = "Дата изменения";
                        worksheet.Cells[1, 2].Value = "Пользователь";
                        worksheet.Cells[1, 3].Value = "Тип действия";
                        worksheet.Cells[1, 4].Value = "ID записи";
                        worksheet.Cells[1, 5].Value = "Поле";
                        worksheet.Cells[1, 6].Value = "Старое значение";
                        worksheet.Cells[1, 7].Value = "Новое значение";

                        // Стиль заголовков
                        using (var range = worksheet.Cells[1, 1, 1, 7])
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        }

                        // Данные
                        int row = 2;
                        foreach (var log in logs)
                        {
                            worksheet.Cells[row, 1].Value = log.FormattedChangedAt;
                            worksheet.Cells[row, 2].Value = log.ChangedBy;
                            worksheet.Cells[row, 3].Value = log.ActionType;
                            worksheet.Cells[row, 4].Value = log.RecordId;

                            // Парсим детали для получения полей
                            if (!string.IsNullOrEmpty(log.Details))
                            {
                                var lines = log.Details.Split('\n');
                                foreach (var line in lines)
                                {
                                    if (line.StartsWith("Поле:"))
                                    {
                                        worksheet.Cells[row, 5].Value = line.Replace("Поле:", "").Trim();
                                    }
                                    else if (line.StartsWith("Старое значение:"))
                                    {
                                        worksheet.Cells[row, 6].Value = line.Replace("Старое значение:", "").Trim();
                                    }
                                    else if (line.StartsWith("Новое значение:"))
                                    {
                                        worksheet.Cells[row, 7].Value = line.Replace("Новое значение:", "").Trim();
                                    }
                                }
                            }

                            // Стиль строк
                            using (var range = worksheet.Cells[row, 1, row, 7])
                            {
                                range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                                range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                            }
                            row++;
                        }

                        // Автоматическая ширина столбцов
                        worksheet.Cells.AutoFitColumns();

                        // Сохранение файла
                        package.SaveAs(new FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Журнал изменений успешно экспортирован", "Успех", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте журнала изменений: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}