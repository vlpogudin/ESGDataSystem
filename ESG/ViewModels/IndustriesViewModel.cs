using ESG.Data;
using ESG.Models;
using ESG.Utilities;
using ESG.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using IndustryItem = ESG.Models.IndustryItem;

namespace ESG.ViewModels
{
    public class IndustriesViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private IndustryItem _selectedIndustry;
        private ObservableCollection<Industry> _allIndustries;
        private ObservableCollection<IndustryItem> _filteredIndustriesForList;
        private ObservableCollection<IndustryItem> _filteredIndustries;
        private string _industrySearchText;
        private ObservableCollection<IndustryItem> _selectedIndustries;

        public IndustriesViewModel()
        {
            _dbService = new DatabaseService();
            _allIndustries = new ObservableCollection<Industry>(_dbService.GetIndustries());
            _filteredIndustriesForList = new ObservableCollection<IndustryItem>();
            _filteredIndustries = new ObservableCollection<IndustryItem>();
            _selectedIndustries = new ObservableCollection<IndustryItem>();

            foreach (var industry in _allIndustries)
            {
                var item = new IndustryItem { IndustryId = industry.IndustryId, IndustryName = industry.IndustryName, IsSelected = false };
                _filteredIndustriesForList.Add(item);
                _filteredIndustries.Add(item);
            }

            AddCommand = new RelayCommand(_ => AddIndustry(), _ => CanPerformCrud);
            EditCommand = new RelayCommand(_ => EditIndustry(), _ => CanPerformCrud && SelectedIndustry != null);
            DeleteCommand = new RelayCommand(_ => DeleteIndustry(), _ => CanPerformCrud && SelectedIndustry != null);
            ClearIndustrySelectionCommand = new RelayCommand(_ => ClearIndustrySelection());
        }

        public ObservableCollection<Industry> AllIndustries
        {
            get => _allIndustries;
            set
            {
                _allIndustries = value;
                OnPropertyChanged(nameof(AllIndustries));
            }
        }

        public ObservableCollection<IndustryItem> FilteredIndustries
        {
            get => _filteredIndustries;
            set
            {
                _filteredIndustries = value;
                OnPropertyChanged(nameof(FilteredIndustries));
            }
        }

        public ObservableCollection<IndustryItem> FilteredIndustriesForList
        {
            get => _filteredIndustriesForList;
            set
            {
                _filteredIndustriesForList = value;
                OnPropertyChanged(nameof(FilteredIndustriesForList));
            }
        }

        public ObservableCollection<IndustryItem> SelectedIndustries
        {
            get => _selectedIndustries;
            set
            {
                _selectedIndustries = value;
                OnPropertyChanged(nameof(SelectedIndustries));
            }
        }

        public IndustryItem SelectedIndustry
        {
            get => _selectedIndustry;
            set
            {
                _selectedIndustry = value;
                OnPropertyChanged(nameof(SelectedIndustry));
            }
        }

        public string IndustrySearchText
        {
            get => _industrySearchText;
            set
            {
                _industrySearchText = value;
                OnPropertyChanged(nameof(IndustrySearchText));
                UpdateFilteredIndustriesForList();
            }
        }

        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ClearIndustrySelectionCommand { get; }

        private void ClearIndustrySelection()
        {
            foreach (var industry in _filteredIndustriesForList)
            {
                industry.IsSelected = false;
            }
            SelectedIndustries.Clear();
            UpdateFilteredIndustries();
        }

        public void UpdateFilteredIndustriesForList()
        {
            var filtered = _allIndustries.Select(i => new IndustryItem { IndustryId = i.IndustryId, IndustryName = i.IndustryName, IsSelected = _filteredIndustriesForList.FirstOrDefault(fi => fi.IndustryName == i.IndustryName)?.IsSelected ?? false });
            if (!string.IsNullOrWhiteSpace(IndustrySearchText))
            {
                filtered = filtered.Where(i => i.IndustryName?.ToLower().Contains(IndustrySearchText.ToLower()) == true);
            }
            FilteredIndustriesForList.Clear();
            foreach (var item in filtered)
            {
                FilteredIndustriesForList.Add(item);
            }
        }

        public void UpdateFilteredIndustries()
        {
            var filtered = _allIndustries.Select(i => new IndustryItem { IndustryId = i.IndustryId, IndustryName = i.IndustryName, IsSelected = _filteredIndustriesForList.FirstOrDefault(fi => fi.IndustryName == i.IndustryName)?.IsSelected ?? false });
            if (SelectedIndustries.Any())
            {
                filtered = filtered.Where(i => SelectedIndustries.Any(si => si.IndustryName == i.IndustryName));
            }
            FilteredIndustries.Clear();
            foreach (var item in filtered)
            {
                FilteredIndustries.Add(item);
            }
            OnPropertyChanged(nameof(FilteredIndustries));
        }

        private void AddIndustry()
        {
            var addWindow = new AddIndustryWindow(null, this);
            if (addWindow.ShowDialog() == true)
            {
                var newIndustry = new Industry { IndustryName = addWindow.IndustryName };
                _dbService.AddIndustry(newIndustry.IndustryName);
                _allIndustries.Clear();
                foreach (var industry in _dbService.GetIndustries())
                {
                    if (!_allIndustries.Any(i => i.IndustryId == industry.IndustryId))
                        _allIndustries.Add(industry);
                }
                UpdateFilteredIndustriesForList();
                UpdateFilteredIndustries();
            }
        }

        private void EditIndustry()
        {
            if (SelectedIndustry != null)
            {
                var originalIndustry = AllIndustries.FirstOrDefault(i => i.IndustryId == SelectedIndustry.IndustryId);
                if (originalIndustry != null)
                {
                    var oldIndustryName = originalIndustry.IndustryName;
                    var editWindow = new AddIndustryWindow(originalIndustry, this);
                    if (editWindow.ShowDialog() == true)
                    {
                        _dbService.UpdateIndustry(oldIndustryName, originalIndustry.IndustryName);
                        _allIndustries.Clear();
                        foreach (var industry in _dbService.GetIndustries())
                        {
                            _allIndustries.Add(industry);
                        }
                        UpdateFilteredIndustries();
                        UpdateFilteredIndustriesForList();
                        OnPropertyChanged(nameof(AllIndustries));
                    }
                }
            }
        }

        private void DeleteIndustry()
        {
            if (SelectedIndustry != null)
            {
                if (MessageBox.Show($"Вы уверены, что хотите удалить отрасль '{SelectedIndustry.IndustryName}'?",
                    "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    _dbService.DeleteIndustry(SelectedIndustry.IndustryName);
                    var industryToRemove = AllIndustries.FirstOrDefault(i => i.IndustryId == SelectedIndustry.IndustryId);
                    if (industryToRemove != null)
                    {
                        AllIndustries.Remove(industryToRemove);
                    }
                    UpdateFilteredIndustries();
                    UpdateFilteredIndustriesForList();
                    SelectedIndustry = null;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}