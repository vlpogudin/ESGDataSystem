using ESG.Data;
using ESG.Models;
using ESG.Utilities;
using ESG.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using IndustryItem = ESG.Models.IndustryItem;

namespace ESG.ViewModels
{
    /// <summary>
    /// Управление списком компаний и операциями над ними
    /// </summary>
    public class IndustriesViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// // Сервис для взаимодействия с базой данных
        /// </summary>
        private readonly DatabaseService _dbService;

        /// <summary>
        /// Выбранная отрасли
        /// </summary>
        private IndustryItem _selectedIndustry;

        /// <summary>
        /// Отрасли
        /// </summary>
        private ObservableCollection<Industry> _allIndustries;

        /// <summary>
        /// Отфильтрованные отрасли по тексту в фильтре
        /// </summary>
        private ObservableCollection<IndustryItem> _filteredIndustriesForList;

        /// <summary>
        /// Отфильтрованные отрасли
        /// </summary>
        private ObservableCollection<IndustryItem> _filteredIndustries;

        /// <summary>
        /// Текст поиска для фильтрации отраслей
        /// </summary>
        private string _industrySearchText;

        /// <summary>
        /// Выбранные отрасли
        /// </summary>
        private ObservableCollection<IndustryItem> _selectedIndustries;

        #endregion

        #region Свойства

        /// <summary>
        /// Отрасли
        /// </summary>
        public ObservableCollection<Industry> AllIndustries
        {
            get => _allIndustries;
            set
            {
                _allIndustries = value;
                OnPropertyChanged(nameof(AllIndustries));
            }
        }

        /// <summary>
        /// Отфильтрованные отрасли
        /// </summary>
        public ObservableCollection<IndustryItem> FilteredIndustries
        {
            get => _filteredIndustries;
            set
            {
                _filteredIndustries = value;
                OnPropertyChanged(nameof(FilteredIndustries));
            }
        }

        /// <summary>
        /// Отфильтрованные отрасли по тексту в фильтре
        /// </summary>
        public ObservableCollection<IndustryItem> FilteredIndustriesForList
        {
            get => _filteredIndustriesForList;
            set
            {
                _filteredIndustriesForList = value;
                OnPropertyChanged(nameof(FilteredIndustriesForList));
            }
        }

        /// <summary>
        /// Выбранные отрасли
        /// </summary>
        public ObservableCollection<IndustryItem> SelectedIndustries
        {
            get => _selectedIndustries;
            set
            {
                _selectedIndustries = value;
                OnPropertyChanged(nameof(SelectedIndustries));
            }
        }

        /// <summary>
        /// Выбранная отрасль
        /// </summary>
        public IndustryItem SelectedIndustry
        {
            get => _selectedIndustry;
            set
            {
                _selectedIndustry = value;
                OnPropertyChanged(nameof(SelectedIndustry));
            }
        }

        /// <summary>
        /// Текст поиска для фильтрации отраслей
        /// </summary>
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

        /// <summary>
        /// Определение разрешений для управления данными
        /// </summary>
        public bool CanPerformCrud => PermissionChecker.CanPerformCrud();

        /// <summary>
        /// Команда добавления отрасли
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// Команда редактирования отрасли
        /// </summary>
        public ICommand EditCommand { get; }

        /// <summary>
        /// Команда удаления отрасли
        /// </summary>
        public ICommand DeleteCommand { get; }

        /// <summary>
        /// Команда очистки отрасли
        /// </summary>
        public ICommand ClearIndustrySelectionCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализауия VM
        /// </summary>
        public IndustriesViewModel()
        {
            _dbService = new DatabaseService();
            _allIndustries = new ObservableCollection<Industry>(_dbService.GetIndustries()); // Загрузка всех отраслей
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
            ClearIndustrySelectionCommand = new RelayCommand(_ => ClearFilters());
        }

        #endregion

        #region Методы

        /// <summary>
        /// Очистка фильтров
        /// </summary>
        private void ClearFilters()
        {
            foreach (var industry in _filteredIndustriesForList)
            {
                industry.IsSelected = false;
            }
            SelectedIndustries.Clear();
            UpdateFilteredIndustries();
        }

        /// <summary>
        /// Обновление списка отфильтрованных отраслей для фильтра
        /// </summary>
        public void UpdateFilteredIndustriesForList()
        {
            // Создание списка отраслей с сохранением состояния выбора
            var filtered = _allIndustries.Select(i => new IndustryItem { 
                IndustryId = i.IndustryId, 
                IndustryName = i.IndustryName, 
                IsSelected = _filteredIndustriesForList.FirstOrDefault(fi => fi.IndustryName == i.IndustryName)?.IsSelected ?? false 
            });
            if (!string.IsNullOrWhiteSpace(IndustrySearchText)) // Применение фильтра по тексту поиска
            {
                filtered = filtered.Where(i => i.IndustryName?.ToLower().Contains(IndustrySearchText.ToLower()) == true);
            }
            FilteredIndustriesForList.Clear();
            foreach (var item in filtered)
            {
                FilteredIndustriesForList.Add(item);
            }
        }

        /// <summary>
        /// Обновление списка отфильтрованных отраслей
        /// </summary>
        public void UpdateFilteredIndustries()
        {
            // Создание списка отраслей с сохранением состояния выбора
            var filtered = _allIndustries.Select(i => new IndustryItem {
                IndustryId = i.IndustryId, 
                IndustryName = i.IndustryName, 
                IsSelected = _filteredIndustriesForList.FirstOrDefault(fi => fi.IndustryName == i.IndustryName)?.IsSelected ?? false 
            });
            if (SelectedIndustries.Any())
            {
                filtered = filtered.Where(i => SelectedIndustries.Any(si => si.IndustryName == i.IndustryName));
            }
            // Обновление коллекции отфильтрованных отраслей
            FilteredIndustries.Clear();
            foreach (var item in filtered)
            {
                FilteredIndustries.Add(item);
            }
            OnPropertyChanged(nameof(FilteredIndustries));
        }

        /// <summary>
        /// Добавление отрасли
        /// </summary>
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

        /// <summary>
        /// Редактирование отрасли
        /// </summary>
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

        /// <summary>
        /// Удаление отрасли
        /// </summary>
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