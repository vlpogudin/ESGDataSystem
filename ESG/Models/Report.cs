using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для отчета компании
    /// </summary>
    public class Report : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код отчета
        /// </summary>
        private int _reportId;

        /// <summary>
        /// Код компании
        /// </summary>
        private int _companyId;

        /// <summary>
        /// Наименование компании
        /// </summary>
        private string _companyName;

        /// <summary>
        /// Наименование отчета
        /// </summary>
        private string _title;

        /// <summary>
        /// Год отчетности
        /// </summary>
        private int? _year;

        /// <summary>
        /// Язык отчета
        /// </summary>
        private string _language;

        /// <summary>
        /// Путь к файлу отчета
        /// </summary>
        private string _filePath;

        /// <summary>
        /// Выбор отчета в фильтрах
        /// </summary>
        private bool _isSelected;

        #endregion

        #region Свойства

        /// <summary>
        /// Код отчета
        /// </summary>
        public int ReportId
        {
            get => _reportId;
            set
            {
                _reportId = value;
                OnPropertyChanged(nameof(ReportId));
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
                _companyId = value;
                OnPropertyChanged(nameof(CompanyId));
            }
        }

        /// <summary>
        /// Наименование компании
        /// </summary>
        public string CompanyName
        {
            get => _companyName;
            set
            {
                _companyName = value;
                OnPropertyChanged(nameof(CompanyName));
            }
        }

        /// <summary>
        /// Наименование отчета
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
        /// Год отчетности
        /// </summary>
        public int? Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
            }
        }

        /// <summary>
        /// Язык отчета
        /// </summary>
        public string Language
        {
            get => _language;
            set
            {
                _language = value;
                OnPropertyChanged(nameof(Language));
            }
        }

        /// <summary>
        /// Путь к файлу отчета
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
            }
        }

        /// <summary>
        /// Выбор отчета в фильтрах
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