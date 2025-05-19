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
    public class WebsitesViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private Website _selectedWebsite;
        private string _companySearchText;
        private string _descriptionSearchText;
        private ObservableCollection<Website> _allWebsites;
        private ObservableCollection<Company> _allCompanies;
        private ObservableCollection<Company> _filteredCompaniesForList;
        private ObservableCollection<Website> _filteredWebsitesForList;

        public WebsitesViewModel()
        {
            _dbService = new DatabaseService();
            _allWebsites = new ObservableCollection<Website>(_dbService.GetWebsites());
            _allCompanies = new ObservableCollection<Company>(_dbService.GetCompanies());
            _filteredCompaniesForList = new ObservableCollection<Company>(_allCompanies);
            _filteredWebsitesForList = new ObservableCollection<Website>(_allWebsites);

            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
            }
            foreach (var website in _allWebsites)
            {
                website.IsSelected = false;
                // Удаляем присваивание CompanyName, так как оно пересчитается автоматически через CompanyId
            }

            FilteredWebsites = new ObservableCollection<Website>(_allWebsites);
            SelectedCompanies = new ObservableCollection<Company>();
            SelectedWebsites = new ObservableCollection<Website>();

            AddCommand = new RelayCommand(_ => AddWebsite(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditWebsite(), _ => CanPerformCrud && SelectedWebsite != null);
            DeleteCommand = new RelayCommand(_ => DeleteWebsite(), _ => CanPerformCrud && SelectedWebsite != null);
            OpenUrlCommand = new RelayCommand(OpenUrl);
            ClearCompanySelectionCommand = new RelayCommand(_ => ClearCompanySelection());
            ClearDescriptionSelectionCommand = new RelayCommand(_ => ClearDescriptionSelection());
            ExportToCsvCommand = new RelayCommand(_ => ExportToCsv());
        }

        public ObservableCollection<Website> FilteredWebsites { get; private set; }

        public ObservableCollection<Company> FilteredCompaniesForList
        {
            get => _filteredCompaniesForList;
            set
            {
                _filteredCompaniesForList = value;
                OnPropertyChanged(nameof(FilteredCompaniesForList));
            }
        }

        public ObservableCollection<Website> FilteredWebsitesForList
        {
            get => _filteredWebsitesForList;
            set
            {
                _filteredWebsitesForList = value;
                OnPropertyChanged(nameof(FilteredWebsitesForList));
            }
        }

        public ObservableCollection<Company> SelectedCompanies { get; set; }

        public ObservableCollection<Website> SelectedWebsites { get; set; }

        public Website SelectedWebsite
        {
            get => _selectedWebsite;
            set
            {
                _selectedWebsite = value;
                OnPropertyChanged(nameof(SelectedWebsite));
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

        public string DescriptionSearchText
        {
            get => _descriptionSearchText;
            set
            {
                _descriptionSearchText = value;
                OnPropertyChanged(nameof(DescriptionSearchText));
                UpdateFilteredWebsitesForList();
            }
        }

        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand OpenUrlCommand { get; }
        public ICommand ClearCompanySelectionCommand { get; }
        public ICommand ClearDescriptionSelectionCommand { get; }
        public ICommand ExportToCsvCommand { get; }

        private void ClearCompanySelection()
        {
            foreach (var company in _allCompanies)
            {
                company.IsSelected = false;
            }
            SelectedCompanies.Clear();
            UpdateFilteredWebsites();
            UpdateFilteredWebsitesForList();
        }

        private void ClearDescriptionSelection()
        {
            foreach (var website in _allWebsites)
            {
                website.IsSelected = false;
            }
            SelectedWebsites.Clear();
            UpdateFilteredWebsites();
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

        public void UpdateFilteredWebsitesForList()
        {
            var filtered = _allWebsites.AsEnumerable();
            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(w => SelectedCompanies.Any(c => c.CompanyId == w.CompanyId));
            }
            if (!string.IsNullOrWhiteSpace(DescriptionSearchText))
            {
                filtered = filtered.Where(w => w.Description?.ToLower().Contains(DescriptionSearchText.ToLower()) == true);
            }
            FilteredWebsitesForList.Clear();
            foreach (var website in filtered)
            {
                FilteredWebsitesForList.Add(website);
            }
        }

        public void UpdateFilteredWebsites()
        {
            var filtered = _allWebsites.AsEnumerable();
            if (SelectedCompanies.Any())
            {
                filtered = filtered.Where(w => SelectedCompanies.Any(c => c.CompanyId == w.CompanyId));
            }
            if (SelectedWebsites.Any())
            {
                filtered = filtered.Where(w => SelectedWebsites.Any(s => s.Description == w.Description));
            }
            FilteredWebsites.Clear();
            foreach (var website in filtered)
            {
                FilteredWebsites.Add(website);
            }
            OnPropertyChanged(nameof(FilteredWebsites));
        }

        private void AddWebsite()
        {
            var window = new AddWebsiteWindow(new Website(), this);
            if (window.ShowDialog() == true)
            {
                if (!window.IsCsvImported)
                {
                    var newWebsite = window.Website;
                    // Удаляем присваивание CompanyName, так как оно пересчитается автоматически через CompanyId
                    AddWebsiteToCollection(newWebsite);
                    FilteredWebsites.Add(newWebsite);
                    UpdateFilteredWebsites();
                    SelectedWebsite = newWebsite;
                }
            }
        }

        private void EditWebsite()
        {
            var window = new EditWebsiteWindow(SelectedWebsite);
            if (window.ShowDialog() == true)
            {
                // Удаляем присваивание CompanyName, так как оно пересчитается автоматически через CompanyId
                _dbService.UpdateWebsite(SelectedWebsite);
                UpdateFilteredWebsites();
            }
        }

        private void DeleteWebsite()
        {
            if (MessageBox.Show($"Вы уверены, что хотите удалить веб-сайт '{SelectedWebsite.Url}'?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _dbService.DeleteWebsite(SelectedWebsite.WebsiteId);
                _allWebsites.Remove(SelectedWebsite);
                _filteredWebsitesForList.Remove(SelectedWebsite);
                UpdateFilteredWebsites();
            }
        }

        private void OpenUrl(object parameter)
        {
            if (parameter is string url)
            {
                try
                {
                    if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult) ||
                        (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
                    {
                        MessageBox.Show("Недействительный URL. Пожалуйста, проверьте адрес.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при открытии URL: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ExportToCsv()
        {
            if (SelectedWebsites == null || !SelectedWebsites.Any())
            {
                MessageBox.Show("Выберите хотя бы один веб-сайт для выгрузки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv",
                FileName = $"WebsitesExport_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var csvBuilder = new StringBuilder();
                    csvBuilder.AppendLine("ID,Компания,URL,Описание,Последнее обновление");

                    foreach (var website in SelectedWebsites)
                    {
                        var id = website.WebsiteId.ToString();
                        var companyName = $"\"{website.CompanyName?.Replace("\"", "\"\"") ?? ""}\"";
                        var url = $"\"{website.Url?.Replace("\"", "\"\"") ?? ""}\"";
                        var description = $"\"{website.Description?.Replace("\"", "\"\"") ?? ""}\"";
                        var lastUpdated = $"\"{website.LastUpdated?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""}\"";
                        csvBuilder.AppendLine($"{id},{companyName},{url},{description},{lastUpdated}");
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

        public void AddWebsiteToCollection(Website website)
        {
            if (!_allWebsites.Any(w => w.WebsiteId == website.WebsiteId))
            {
                // Удаляем присваивание CompanyName, так как оно пересчитается автоматически через CompanyId
                _allWebsites.Add(website);
                _filteredWebsitesForList.Add(website);
            }
        }

        public void RefreshWebsitesFromDatabase()
        {
            _allWebsites.Clear();
            foreach (var website in _dbService.GetWebsites())
            {
                // Удаляем присваивание CompanyName, так как оно пересчитается автоматически через CompanyId
                _allWebsites.Add(website);
            }
            UpdateFilteredWebsites();
            UpdateFilteredWebsitesForList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}