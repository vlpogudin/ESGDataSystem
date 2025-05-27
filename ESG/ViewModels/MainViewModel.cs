using ESG.Data;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using ESG.Views;

namespace ESG.ViewModels
{
    /// <summary>
    /// Управление навигацией между страницами
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Сервис для взаимодействия с базой данных
        /// </summary>
        private readonly DatabaseService _dbService;

        /// <summary>
        /// Текущая отображаемая система
        /// </summary>
        private Page _currentPage;

        public delegate void LogoutEventHandler();
        public event LogoutEventHandler LogoutRequested;
        #endregion

        #region Свойства

        /// <summary>
        /// Текущая отображаемая система
        /// </summary>
        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        /// <summary>
        /// Команда перехода на страницу компаний
        /// </summary>
        public ICommand NavigateToCompaniesCommand { get; }

        /// <summary>
        /// Команда перехода на страницу отчетов
        /// </summary>
        public ICommand NavigateToReportsCommand { get; }

        /// <summary>
        /// Команда перехода на страницу веб-сайтов
        /// </summary>
        public ICommand NavigateToWebsitesCommand { get; }

        /// <summary>
        /// Команда перехода на страницу новостей
        /// </summary>
        public ICommand NavigateToNewsCommand { get; }

        /// <summary>
        /// Команда перехода на страницу пользователей
        /// </summary>
        public ICommand NavigateToUsersCommand { get; }

        /// <summary>
        /// Команда перехода на страницу выгрузки данных
        /// </summary>
        public ICommand NavigateToExportDataCommand { get; }

        /// <summary>
        /// Команда выхода из профиля
        /// </summary>
        public ICommand LogoutCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация VM
        /// </summary>
        public MainViewModel()
        {
            _dbService = new DatabaseService();
            if (!_dbService.IsConnected())
            {
                MessageBox.Show("Не удалось подключиться к серверу. Убедитесь, что вы подключены к локальной сети компании.", 
                    "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
                return;
            }


            // Инициализация команд
            NavigateToCompaniesCommand = new RelayCommand(_ => NavigateTo(new CompaniesPage()));
            NavigateToReportsCommand = new RelayCommand(_ => NavigateTo(new ReportsPage()));
            NavigateToWebsitesCommand = new RelayCommand(_ => NavigateTo(new WebsitesPage()));
            NavigateToNewsCommand = new RelayCommand(_ => NavigateTo(new NewsPage()));
            NavigateToUsersCommand = new RelayCommand(_ => NavigateTo(new UsersPage()));
            NavigateToExportDataCommand = new RelayCommand(_ => NavigateTo(new ExportDataPage()));
            LogoutCommand = new RelayCommand(_ => Logout());

            // Открываем страницу компаний по умолчанию
            CurrentPage = new CompaniesPage();
        }

        #endregion

        #region Методы

        /// <summary>
        /// Переход на указанную страницу
        /// </summary>
        /// <param name="page"></param>
        private void NavigateTo(Page page)
        {
            CurrentPage = page;
        }

        /// <summary>
        /// Выход из профиля
        /// </summary>
        private void Logout()
        {
            if (MessageBox.Show("Вы уверены, что хотите сменить профиль?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                LogoutRequested?.Invoke(); // Вызываем событие выхода
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