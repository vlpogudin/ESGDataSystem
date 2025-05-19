using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для роли пользователя
    /// </summary>
    public class Role : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код роли
        /// </summary>
        private int _roleId;

        /// <summary>
        /// Наименование роли
        /// </summary>
        private string _roleName;

        /// <summary>
        /// Описание роли
        /// </summary>
        private string _description;

        #endregion

        #region Свойства

        /// <summary>
        /// Код роли
        /// </summary>
        public int RoleId
        {
            get => _roleId;
            set
            {
                _roleId = value;
                OnPropertyChanged(nameof(RoleId));
            }
        }

        /// <summary>
        /// Наименование роли
        /// </summary>
        public string RoleName
        {
            get => _roleName;
            set
            {
                _roleName = value;
                OnPropertyChanged(nameof(RoleName));
            }
        }

        /// <summary>
        /// Описание роли
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