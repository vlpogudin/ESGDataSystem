using ESG.Data;
using ESG.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ESG.Views
{
    /// <summary>
    /// Логика взаимодействия для AddReportWindow.xaml
    /// </summary>
    public partial class AddReportWindow : Window, INotifyPropertyChanged
    {
        private readonly Report _report;
        private readonly DatabaseService _dbService;
        private ObservableCollection<Company> _companies;
        private ObservableCollection<Company> _filteredCompanies;
        private string _companySearchText;
        private ObservableCollection<YearItem> _years;
        private YearItem _selectedYear;

        public AddReportWindow(Report report)
        {
            InitializeComponent();
            _report = report;
            _dbService = new DatabaseService();
            _companies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompanies = new ObservableCollection<Company>(_companies);
            _years = new ObservableCollection<YearItem>(GenerateYears());
            CompanySearchText = string.Empty;

            // Устанавливаем DataContext для содержимого окна (ScrollViewer) на _report
            var scrollViewer = Content as ScrollViewer;
            if (scrollViewer != null)
            {
                scrollViewer.DataContext = _report;
            }
        }

        public ObservableCollection<Company> Companies
        {
            get => _companies;
            set => _companies = value;
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
                UpdateFilteredCompanies(); // Обновляем список компаний при изменении текста поиска
            }
        }

        public ObservableCollection<YearItem> Years
        {
            get => _years;
            set
            {
                _years = value;
                OnPropertyChanged(nameof(Years));
            }
        }

        public YearItem SelectedYear
        {
            get => _selectedYear;
            set
            {
                _selectedYear = value;
                _report.Year = value?.Year; // Устанавливаем год в Report
                OnPropertyChanged(nameof(SelectedYear));
            }
        }

        public Report Report => _report;

        private void UpdateFilteredCompanies()
        {
            var filtered = Companies.AsEnumerable();

            // Фильтрация по тексту поиска
            if (!string.IsNullOrWhiteSpace(CompanySearchText))
            {
                filtered = filtered.Where(c => c.Name?.ToLower().Contains(CompanySearchText.ToLower()) == true);
            }

            // Сортировка: выбранная компания (если есть) идёт первой
            var selectedCompany = Companies.FirstOrDefault(c => c.IsSelected);
            if (selectedCompany != null)
            {
                filtered = filtered.OrderBy(c => c.CompanyId != selectedCompany.CompanyId);
            }

            FilteredCompanies.Clear();
            foreach (var company in filtered)
            {
                FilteredCompanies.Add(company);
            }
        }

        private List<YearItem> GenerateYears()
        {
            var years = new List<YearItem>();
            int currentYear = DateTime.Now.Year;
            for (int year = 2000; year <= currentYear; year++)
            {
                years.Add(new YearItem { Year = year });
            }
            return years;
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                // Снимаем выбор с других компаний
                foreach (var c in Companies.Where(c => c != company))
                {
                    c.IsSelected = false;
                }
                company.IsSelected = true;
                _report.CompanyId = company.CompanyId;
                UpdateFilteredCompanies(); // Обновляем список, чтобы выбранная компания была сверху
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                company.IsSelected = false;
                if (company.CompanyId == _report.CompanyId)
                {
                    _report.CompanyId = 0; // Сбрасываем, если выбранная компания снята
                }
                UpdateFilteredCompanies();
            }
        }

        private void ClearCompanySelectionCommand(object parameter)
        {
            foreach (var company in Companies)
            {
                company.IsSelected = false;
            }
            _report.CompanyId = 0;
            UpdateFilteredCompanies();
        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "All files (*.*)|*.*",
                Title = "Выберите файл отчёта"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                _report.FilePath = openFileDialog.FileName;
                // Принудительно обновляем текстовое поле
                var filePathTextBox = this.FindName("FilePathTextBox") as TextBox;
                if (filePathTextBox != null)
                {
                    filePathTextBox.Text = _report.FilePath;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_report.Title))
            {
                MessageBox.Show("Пожалуйста, укажите название отчета.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (_report.CompanyId <= 0)
            {
                MessageBox.Show("Пожалуйста, выберите компанию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}