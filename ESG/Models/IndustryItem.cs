using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для текущей отрасли
    /// </summary>
    public class IndustryItem : INotifyPropertyChanged
    {
        #region Поля
        /// <summary>
        /// Код отрасли
        /// </summary>
        private int _industryId;

        /// <summary>
        /// Наименование отрасли
        /// </summary>
        private string _industryName;

        /// <summary>
        /// Выбор отрасли для фильтра
        /// </summary>
        private bool _isSelected;

        #endregion

        #region Свойства

        /// <summary>
        /// Код отрасли
        /// </summary>
        public int IndustryId
        {
            get => _industryId;
            set
            {
                _industryId = value;
                OnPropertyChanged(nameof(IndustryId));
            }
        }

        /// <summary>
        /// Наименование отрасли
        /// </summary>
        public string IndustryName
        {
            get => _industryName;
            set
            {
                _industryName = value;
                OnPropertyChanged(nameof(IndustryName));
            }
        }

        /// <summary>
        /// Выбор отрасли для фильтра
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