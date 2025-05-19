using ESG.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using ESG.Views;

namespace ESG.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private Page _currentPage;

        public MainViewModel()
        {
            _dbService = new DatabaseService();

            // Проверка подключения
            if (!_dbService.IsConnected())
            {
                MessageBox.Show("Не удалось подключиться к серверу. Убедитесь, что вы подключены к локальной сети компании.", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
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

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }

        public ICommand NavigateToCompaniesCommand { get; }
        public ICommand NavigateToReportsCommand { get; }
        public ICommand NavigateToWebsitesCommand { get; }
        public ICommand NavigateToNewsCommand { get; }
        public ICommand NavigateToUsersCommand { get; }
        public ICommand NavigateToExportDataCommand { get; }
        public ICommand LogoutCommand { get; }

        private void NavigateTo(Page page)
        {
            CurrentPage = page;
        }

        private void Logout()
        {
            MessageBox.Show("Выход из профиля (в разработке).");
            Application.Current.Shutdown();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
