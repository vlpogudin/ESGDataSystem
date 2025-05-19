using ESG.Data;
using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для веб-сайтов
    /// </summary>
    public class Website : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код веб-сайта
        /// </summary>
        private int _websiteId;

        /// <summary>
        /// Код компании
        /// </summary>
        private int _companyId;

        /// <summary>
        /// Наименование компании
        /// </summary>
        private string _companyName;

        /// <summary>
        /// Ссылка на веб-сайт
        /// </summary>
        private string _url;

        /// <summary>
        /// Описание сайта/раздел
        /// </summary>
        private string _description;

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        private DateTime? _lastUpdated;

        /// <summary>
        /// Выбор веб-сайта для фильтра
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// Ссылка на сервис базы данных для получения данных компании
        /// </summary>
        private readonly DatabaseService _dbService;

        #endregion

        #region Свойства

        /// <summary>
        /// Код веб-сайта
        /// </summary>
        public int WebsiteId
        {
            get => _websiteId;
            set
            {
                _websiteId = value;
                OnPropertyChanged(nameof(WebsiteId));
            }
        }

        /// <summary>
        /// Код компании
        /// </summary>
        public int CompanyId
        {
            get => _companyId;
            set
            {
                if (_companyId != value)
                {
                    _companyId = value;
                    _companyName = null; // Сбрасываем кэш
                    OnPropertyChanged(nameof(CompanyId));
                    OnPropertyChanged(nameof(CompanyName)); // Уведомляем об изменении CompanyName
                }
            }
        }

        /// <summary>
        /// Наименование компании
        /// </summary>
        public string CompanyName
        {
            get
            {
                if (string.IsNullOrEmpty(_companyName) && CompanyId > 0)
                {
                    _companyName = _dbService.GetCompanyNameById(CompanyId) ?? "Неизвестно";
                }
                return _companyName;
            }
        }

        /// <summary>
        /// Ссылка на веб-сайт
        /// </summary>
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                OnPropertyChanged(nameof(Url));
            }
        }

        /// <summary>
        /// Описание сайта/раздел
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged(nameof(Description));
            }
        }

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime? LastUpdated
        {
            get => _lastUpdated;
            set
            {
                _lastUpdated = value;
                OnPropertyChanged(nameof(LastUpdated));
            }
        }

        /// <summary>
        /// Выбор веб-сайта для фильтра
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация веб-сайтов
        /// </summary>
        public Website()
        {
            _dbService = new DatabaseService();
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}