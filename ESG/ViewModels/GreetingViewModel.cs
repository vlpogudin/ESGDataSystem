using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using ESG.Models;
using ESG.Views;

namespace ESG.ViewModels
{
    /// <summary>
    /// Главная страница приложения с переходами на другие разделы
    /// </summary>
    public class GreetingViewModel : INotifyPropertyChanged
    {
        #region Поля

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        private User _currentUser;

        /// <summary>
        /// Основная область страницы
        /// </summary>
        private readonly Frame _mainFrame;

        #endregion

        #region Свойства

        /// <summary>
        /// Текущий пользователь
        /// </summary>
        public User CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Команда навигации к странице компаний
        /// </summary>
        public ICommand NavigateToCompaniesCommand { get; }

        /// <summary>
        /// Команда навигации к странице отчетов
        /// </summary>
        public ICommand NavigateToReportsCommand { get; }

        /// <summary>
        /// Команда навигации к странице новостей
        /// </summary>
        public ICommand NavigateToNewsCommand { get; }

        /// <summary>
        /// Команда навигации к странице веб-сайтов
        /// </summary>
        public ICommand NavigateToWebsitesCommand { get; }

        /// <summary>
        /// Команда навигации к странице выгрузки данных
        /// </summary>
        public ICommand NavigateToExportDataCommand { get; }

        #endregion

        #region Конструкторы

        /// <summary>
        /// Инициализация страницы
        /// </summary>
        /// <param name="currentUser"></param>
        /// <param name="mainFrame"></param>
        public GreetingViewModel(User currentUser, Frame mainFrame)
        {
            _currentUser = currentUser;
            _mainFrame = mainFrame;
            // Подписка на команды
            NavigateToCompaniesCommand = new RelayCommand(_ => NavigateToCompanies());
            NavigateToReportsCommand = new RelayCommand(_ => NavigateToReports());
            NavigateToNewsCommand = new RelayCommand(_ => NavigateToNews());
            NavigateToWebsitesCommand = new RelayCommand(_ => NavigateToWebsites());
            NavigateToExportDataCommand = new RelayCommand(_ => NavigateToExportData());
        }

        #endregion

        #region Методы

        /// <summary>
        /// Метод навигации к странице компаний
        /// </summary>
        private void NavigateToCompanies() => _mainFrame?.Navigate(new CompaniesPage());

        /// <summary>
        /// Метод навигации к странице отчетов
        /// </summary>
        private void NavigateToReports() => _mainFrame?.Navigate(new ReportsPage());

        /// <summary>
        /// Метод навигации к странице новостей
        /// </summary>
        private void NavigateToNews() => _mainFrame?.Navigate(new NewsPage());

        /// <summary>
        /// Метод навигации к странице веб-сайтов
        /// </summary>
        private void NavigateToWebsites() => _mainFrame?.Navigate(new WebsitesPage());

        /// <summary>
        /// Метод навигации к странице выгрузки данных
        /// </summary>
        private void NavigateToExportData() => _mainFrame?.Navigate(new ExportDataPage());

        /// <summary>
        /// Вызов события изменения свойства
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Метод для вызова события изменения свойства
        /// </summary>
        /// <param name="propertyName"></param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}