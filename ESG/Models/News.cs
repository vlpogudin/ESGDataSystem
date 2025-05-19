using ESG.Data;
using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для новостей компании
    /// </summary>
    public class News : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код новости
        /// </summary>
        private int _newsId;

        /// <summary>
        /// Код компании
        /// </summary>
        private int _companyId;

        /// <summary>
        /// Наименование компании
        /// </summary>
        private string _companyName;

        /// <summary>
        /// Заголовок новости
        /// </summary>
        private string _title;

        /// <summary>
        /// Текст новости
        /// </summary>
        private string _content;

        /// <summary>
        /// Дата публикации новости
        /// </summary>
        private DateTime? _date;

        /// <summary>
        /// Источник новости
        /// </summary>
        private string _source;

        /// <summary>
        /// Выбор новости для фильтра
        /// </summary>
        private bool _isSelected;

        /// <summary>
        /// Ссылка на сервис базы данных для получения данных компании
        /// </summary>
        private readonly DatabaseService _dbService;

        #endregion

        #region Свойства

        /// <summary>
        /// Код новости
        /// </summary>
        public int NewsId
        {
            get => _newsId;
            set
            {
                _newsId = value;
                OnPropertyChanged(nameof(NewsId));
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
                    OnPropertyChanged(nameof(CompanyName));
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
            set
            {
                _companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        /// <summary>
        /// Заголовок новости
        /// </summary>
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        /// <summary>
        /// Текст новости
        /// </summary>
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        /// <summary>
        /// Дата публикации новости
        /// </summary>
        public DateTime? Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        /// <summary>
        /// Источник новости
        /// </summary>
        public string Source
        {
            get => _source;
            set
            {
                _source = value;
                OnPropertyChanged(nameof(Source));
            }
        }

        /// <summary>
        /// Выбор новости для фильтра
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
        /// Инициализация экземпляра
        /// </summary>
        public News()
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