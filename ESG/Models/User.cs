using System.ComponentModel;

namespace ESG.Models
{
    /// <summary>
    /// Модель данных для пользователей
    /// </summary>
    public class User : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Код пользователя
        /// </summary>
        private int _userId;

        /// <summary>
        /// Логин пользователя
        /// </summary>
        private string _username;

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        private string _password;

        /// <summary>
        /// Код роли
        /// </summary>
        private int _roleId;

        /// <summary>
        /// Список разрешений для пользователя
        /// </summary>
        private List<Permission> _permissions;

        #endregion

        #region Свойства

        /// <summary>
        /// Код пользователя
        /// </summary>
        public int UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                OnPropertyChanged(nameof(UserId));
            }
        }

        /// <summary>
        /// Логин пользователя
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

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
                OnPropertyChanged(nameof(Role));
            }
        }

        /// <summary>
        /// Список разрешений для пользователя
        /// </summary>
        public List<Permission> Permissions
        {
            get => _permissions ??= new List<Permission>();
            set
            {
                _permissions = value;
                OnPropertyChanged(nameof(Permissions));
            }
        }

        /// <summary>
        /// Роли пользователей
        /// </summary>
        public string Role
        {
            get
            {
                return RoleId switch
                {
                    1 => "Администратор",
                    2 => "Модератор данных",
                    3 => "Аналитик",
                    _ => "Неизвестная роль"
                };
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

    /// <summary>
    /// Разрешения
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Код разрешения
        /// </summary>
        public int PermissionId { get; set; }

        /// <summary>
        /// Код роли
        /// </summary>
        public int RoleId { get; set; }

        /// <summary>
        /// Наименование разрешения
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// Описание разрешения
        /// </summary>
        public string Description { get; set; }
    }
}