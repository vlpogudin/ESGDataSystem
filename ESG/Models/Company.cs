using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для компании
    /// </summary>
    public class Company : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код компании
        /// </summary>
        private int _companyId;

        /// <summary>
        /// Наименование компании
        /// </summary>
        private string _name;

        /// <summary>
        /// Страна компании
        /// </summary>
        private string _country;

        /// <summary>
        /// Выбор компании для фильтра
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// Обновление компании
        /// </summary>
        private bool _isUpdating;

        /// <summary>
        /// Список выбранных компаний
        /// </summary>
        private List<string> _selectedIndustries = new List<string>();

        #endregion

        #region Свойства

        /// <summary>
        /// Код компании
        /// </summary>
        public int CompanyId
        {
            get => _companyId;
            set
            {
                _companyId = value;
                OnPropertyChanged(nameof(CompanyId));
            }
        }

        /// <summary>
        /// Наименование компании
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Страна компании
        /// </summary>
        public string Country
        {
            get => _country;
            set
            {
                _country = value;
                OnPropertyChanged(nameof(Country));
            }
        }

        /// <summary>
        /// Выбор компании для фильтра
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!_isUpdating && _isSelected != value) // Проверка на рекурсию
                {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                }
            }
        }

        /// <summary>
        /// Список выбранных компаний
        /// </summary>
        public List<string> SelectedIndustries
        {
            get => _selectedIndustries;
            set
            {
                _selectedIndustries = value;
                OnPropertyChanged(nameof(SelectedIndustries));
                OnPropertyChanged(nameof(IndustriesString));
            }
        }

        /// <summary>
        /// Список веб-сайтов компании
        /// </summary>
        public List<Website> Websites { get; set; } = new List<Website>();

        /// <summary>
        /// Список доступных компаний
        /// </summary>
        public List<string> AvailableIndustries { get; set; } = new List<string>();

        /// <summary>
        /// Представление кода отраслей как наименований отраслей
        /// </summary>
        public string IndustriesString
        {
            get => SelectedIndustries.Any() ? string.Join(", ", SelectedIndustries) : "N/A";
        }

        #endregion

        #region Методы

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
            if (PropertyChanged != null && !_isUpdating)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Метод для обновления без рекурсии
        /// </summary>
        /// <param name="value"></param>
        public void SetIsSelectedWithoutNotification(bool value)
        {
            _isUpdating = true;
            _isSelected = value;
            _isUpdating = false;
        }

        #endregion
    }
}