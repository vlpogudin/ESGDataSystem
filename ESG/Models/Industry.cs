using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для отраслей
    /// </summary>
    public class Industry : INotifyPropertyChanged
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