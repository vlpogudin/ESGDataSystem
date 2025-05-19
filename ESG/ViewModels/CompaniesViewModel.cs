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

namespace ESG.ViewModels
{
    /// <summary>
    /// Управление списком компаний и операциями над ними
    /// </summary>
    public class CompaniesViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Сервис для взаимодействия с базой данных
        /// </summary>
        private readonly DatabaseService _dbService;

        /// <summary>
        /// Выбранная компания в таблице
        /// </summary>
        private Company _selectedCompany;

        /// <summary>
        /// Полный список всех компаний из базы данных
        /// </summary>
        private ObservableCollection<Company> _allCompanies;

        /// <summary>
        /// Отфильтрованный список компаний
        /// </summary>
        private ObservableCollection<Company> _filteredCompanies;

        /// <summary>
        /// Доступный список компаний для фильтрации
        /// </summary>
        private ObservableCollection<Company> _availableCompanies;

        /// <summary>
        /// Список выбранных компаний
        /// </summary>
        private ObservableCollection<Company> _selectedCompanies;

        /// <summary>
        /// Отфильтрованный список для выпадающего списка
        /// </summary>
        private ObservableCollection<Company> _filteredForList;

        /// <summary>
        /// Текст для поиска компаний
        /// </summary>
        private string _companySearchText;

        #endregion

        #region Свойства

        /// <summary>
        /// Отфильтрованный список компаний
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
        /// Доступный список компаний
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
        /// Отфильтрованный список для выпадающего списка
        /// </summary>
        public ObservableCollection<Company> FilteredForList
        {
            get => _filteredForList;
            set
            {
                _filteredForList = value;
                OnPropertyChanged(nameof(FilteredForList));
            }
        }

        /// <summary>
        /// Список выбранных компаний
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
        /// Выбранная компания
        /// </summary>
        public Company SelectedCompany
        {
            get => _selectedCompany;
            set
            {
                _selectedCompany = value;
                OnPropertyChanged(nameof(SelectedCompany));
            }
        }

        /// <summary>
        /// Поиска компаний с применением фильтрации
        /// </summary>
        public string CompanySearchText
        {
            get => _companySearchText;
            set
            {
                _companySearchText = value;
                OnPropertyChanged(nameof(CompanySearchText));
                UpdateFilteredForList();
                UpdateTableCompanies();
            }
        }

        /// <summary>
        /// Проверка прав пользователя на выполнение CRUD-операций
        /// </summary>
        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();

        /// <summary>
        /// Команда для добавления новой компании
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Команда для редактирования выбранной компании
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// Команда для удаления выбранной компании
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Команда для открытия веб-сайта компании
        /// </summary>
        public ICommand OpenWebsiteCommand { get; }

        /// <summary>
        /// Команда для экспорта выбранных компаний в CSV
        /// </summary>
        public ICommand ExportToCsvCommand { get; }

        /// <summary>
        /// Команда для очистки выбора компаний
        /// </summary>
        public ICommand ClearSelectionCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация VM
        /// </summary>
        public CompaniesViewModel()
        {
            // Инициализация и загрузка записей
            _dbService = new DatabaseService(); // Инициализация сервиса базы данных
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies()); // Загрузка компаний из базы данных
            foreach (var company in _allCompanies) // Загрузка веб-сайтов для компаний
            {
                company.Websites = _dbService.GetWebsitesForCompany(company.CompanyId);
            }
            AvailableCompanies = new ObservableCollection<Company>(_allCompanies);
            FilteredForList = new ObservableCollection<Company>(AvailableCompanies);
            FilteredCompanies = new ObservableCollection<Company>(AvailableCompanies);
            SelectedCompanies = new ObservableCollection<Company>();

            // Доступные команды для пользователей
            AddCommand = new RelayCommand(_ => AddCompany(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditCompany(), _ => CanPerformCrud && SelectedCompany != null);
            DeleteCommand = new RelayCommand(_ => DeleteCompany(), _ => CanPerformCrud && SelectedCompany != null);
            OpenWebsiteCommand = new RelayCommand(OpenWebsite);
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
            ClearSelectionCommand = new RelayCommand(_ => ClearSelection());
            CompanySearchText = string.Empty;

            // Подписка на изменение выбранных компаний
            SelectedCompanies.CollectionChanged += (s, e) => UpdateTableCompanies();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Обновление отфильтрованного списка на основе текста поиска
        /// </summary>
        private void UpdateFilteredForList()
        {
            var filtered = ApplyFiltering(AvailableCompanies.ToList());
            FilteredForList = new ObservableCollection<Company>(filtered);
            var toRemove = SelectedCompanies.Where(sc => !AvailableCompanies.Contains(sc)).ToList();
            foreach (var company in toRemove)
            {
                SelectedCompanies.Remove(company); // Удаление выбранной компании
                company.IsSelected = false;
            }
        }

        /// <summary>
        /// Обновление таблицы компаний на основе выбранных и отфильтрованных данных
        /// </summary>
        private void UpdateTableCompanies()
        {
            if (SelectedCompanies.Any())
            {
                FilteredCompanies = new ObservableCollection<Company>(ApplyFiltering(SelectedCompanies.ToList()));
            }
            else
            {
                FilteredCompanies = new ObservableCollection<Company>(ApplyFiltering(AvailableCompanies.ToList()));
            }
            OnPropertyChanged(nameof(FilteredCompanies));
        }

        /// <summary>
        /// Применение фильтрации к списку компаний по тексту поиска
        /// </summary>
        /// <param name="companies">Исходный список компаний</param>
        /// <returns>Отфильтрованный список компаний</returns>
        private List<Company> ApplyFiltering(List<Company> companies)
        {
            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                return companies
                    .Where(c => c.Name?.Contains(CompanySearchText, StringComparison.OrdinalIgnoreCase) == true)
                    .ToList();
            }
            return companies;
        }

        /// <summary>
        /// Обновление всех отфильтрованных списков компаний
        /// </summary>
        public void UpdateFilteredCompanies()
        {
            UpdateFilteredForList();
            UpdateTableCompanies();
        }

        /// <summary>
        /// Добавление новой компании
        /// </summary>
        private void AddCompany()
        {
            var addWindow = new AddCompanyWindow();
            if (addWindow.ShowDialog() == true)
            {
                var newCompany = addWindow.Company; // Создание новой компании
                newCompany.Websites = _dbService.GetWebsitesForCompany(newCompany.CompanyId); // Загрузка веб-сайтов компании
                // Обновление и загрузка всех компаний
                _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
                foreach (var company in _allCompanies)
                {
                    company.Websites = _dbService.GetWebsitesForCompany(company.CompanyId);
                }
                AvailableCompanies = new ObservableCollection<Company>(_allCompanies);
                UpdateFilteredCompanies(); 
                SelectedCompany = _allCompanies.FirstOrDefault(c => c.CompanyId == newCompany.CompanyId);
            }
        }

        /// <summary>
        /// Редактирование выбранной компании
        /// </summary>
        private void EditCompany()
        {
            var editWindow = new EditCompanyWindow(SelectedCompany);
            if (editWindow.ShowDialog() == true)
            {
                _dbService.UpdateCompany(SelectedCompany); // Обновление записи в базе данных
                SelectedCompany.Websites = _dbService.GetWebsitesForCompany(SelectedCompany.CompanyId);
                _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
                foreach (var company in _allCompanies)
                {
                    company.Websites = _dbService.GetWebsitesForCompany(company.CompanyId);
                }
                AvailableCompanies = new ObservableCollection<Company>(_allCompanies);
                UpdateFilteredCompanies();
            }
        }

        /// <summary>
        /// Удаление выбранной компании
        /// </summary>
        private void DeleteCompany()
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить компанию '{SelectedCompany.Name}'?",
                "Удаление компании", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _dbService.DeleteCompany(SelectedCompany.CompanyId); // Удаление компании из базы данных
                _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
                foreach (var company in _allCompanies)
                {
                    company.Websites = _dbService.GetWebsitesForCompany(company.CompanyId);
                }
                AvailableCompanies = new ObservableCollection<Company>(_allCompanies);
                UpdateFilteredCompanies();
            }
        }

        /// <summary>
        /// Открытие веб-сайта компании в браузере
        /// </summary>
        /// <param name="parameter">URL веб-сайта</param>
        private void OpenWebsite(object parameter)
        {
            if (parameter is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии сайта: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Экспорт выбранных компаний в файл CSV
        /// </summary>
        private void ExportToCsv()
        {
            // Если компании в списке не выбраны
            if (SelectedCompanies == null || !SelectedCompanies.Any())
            {
                MessageBox.Show("Выберите как минимум одну компанию для выгрузки в списке.", 
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Настройка выгружаемого файла и окна с выгрузкой
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"Companies_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };
            // Окно с выгрузкой
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var csvBuilder = new StringBuilder();
                    BuildCsvHeader(csvBuilder); // Создание заголовков CSV
                    BuildCsvRows(csvBuilder); // Создание основных данных CSV
                    File.WriteAllText(saveFileDialog.FileName, csvBuilder.ToString(), Encoding.UTF8);
                    MessageBox.Show("Данные выгружены в CSV!", "Выгрузка данных", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте: {ex.Message}", 
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Создание заголовка CSV
        /// </summary>
        /// <param name="csvBuilder"></param>
        private void BuildCsvHeader(StringBuilder csvBuilder)
        {
            csvBuilder.AppendLine("ID,Название,Отрасли,Страна,Веб-сайт,Описание веб-сайта,Последнее обновление");
        }

        /// <summary>
        /// Создание строк данных для CSV
        /// </summary>
        /// <param name="csvBuilder"></param>
        private void BuildCsvRows(StringBuilder csvBuilder)
        {
            foreach (var company in SelectedCompanies)
            {
                if (company.Websites == null || !company.Websites.Any())
                {
                    var id = company.CompanyId.ToString();
                    var name = $"\"{company.Name?.Replace("\"", "\"\"") ?? ""}\"";
                    var industries = $"\"{string.Join(", ", company.SelectedIndustries?.Select(i => i.Replace("\"", "\"\"")) ?? new List<string>())}\"";
                    var country = $"\"{company.Country?.Replace("\"", "\"\"") ?? ""}\"";
                    csvBuilder.AppendLine($"{id},{name},{industries},{country},\"\",\"\",\"\"");
                }
                else
                {
                    foreach (var website in company.Websites)
                    {
                        var id = company.CompanyId.ToString();
                        var name = $"\"{company.Name?.Replace("\"", "\"\"") ?? ""}\"";
                        var industries = $"\"{string.Join(", ", company.SelectedIndustries?.Select(i => i.Replace("\"", "\"\"")) ?? new List<string>())}\"";
                        var country = $"\"{company.Country?.Replace("\"", "\"\"") ?? ""}\"";
                        var url = $"\"{website.Url?.Replace("\"", "\"\"") ?? ""}\"";
                        var description = $"\"{website.Description?.Replace("\"", "\"\"") ?? ""}\"";
                        var lastUpdated = $"\"{website.LastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}\"";
                        csvBuilder.AppendLine($"{id},{name},{industries},{country},{url},{description},{lastUpdated}");
                    }
                }
            }
        }

        /// <summary>
        /// Очистка выбора компаний
        /// </summary>
        private void ClearSelection()
        {
            foreach (var company in SelectedCompanies.ToList())
            {
                company.IsSelected = false;
                SelectedCompanies.Remove(company);
            }
            UpdateTableCompanies();
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