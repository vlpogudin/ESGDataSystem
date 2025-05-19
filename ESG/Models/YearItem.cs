using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для года
    /// </summary>
    public class YearItem : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Год
        /// </summary>
        private int _year;

        #endregion

        #region Свойства

        /// <summary>
        /// Год
        /// </summary>
        public int Year
        {
            get => _year;
            set
            {
                _year = value;
                OnPropertyChanged(nameof(Year));
                OnPropertyChanged(nameof(ToString)); // Уведомление для переопределенного ToString
            }
        }

        #endregion

        #region Методы

        /// <summary>
        /// Преобразование года в строку
        /// </summary>
        /// <returns>Строковое представление года</returns>
        public override string ToString()
        {
            return Year.ToString();
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
