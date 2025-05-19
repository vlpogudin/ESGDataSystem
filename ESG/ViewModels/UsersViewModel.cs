using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using ESG.Data;
using ESG.Models;
using ESG.Views;
using System.Linq;
using ESG.Utilities;

namespace ESG.ViewModels
{
    public class UsersViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _dbService;
        private User _selectedUser;
        private string _searchText;
        private ObservableCollection<User> _allUsers;
        private ObservableCollection<User> _filteredUsers;
        private ObservableCollection<Role> _roles;
        public static User CurrentUser { get; set; }

        public UsersViewModel()
        {
            _dbService = new DatabaseService();
            _allUsers = new ObservableCollection<User>(_dbService.GetUsers());
            Users = new ObservableCollection<User>(_allUsers);
            Roles = new ObservableCollection<Role>(_dbService.GetRoles());

            AddUserCommand = new RelayCommand(_ => AddUser(), _ => CanAddUser());
            EditUserCommand = new RelayCommand(_ => EditUser(), _ => CanUpdateUser());
            DeleteUserCommand = new RelayCommand(_ => DeleteUser(), _ => CanDeleteUser());
            ClearSearchCommand = new RelayCommand(_ => SearchText = string.Empty);

            // Проверка прав при загрузке
            if (CurrentUser == null || !PermissionChecker.CanManageUsers())
            {
                MessageBox.Show("У вас нет прав для доступа к этой странице.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                // Закрытие или редирект (нужно реализовать в MainWindow)
            }
        }

        public ObservableCollection<User> Users
        {
            get => _filteredUsers;
            private set
            {
                _filteredUsers = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Role> Roles
        {
            get => _roles;
            private set
            {
                _roles = value;
                OnPropertyChanged();
            }
        }

        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                _selectedUser = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterUsers();
            }
        }

        public ICommand AddUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }
        public ICommand ClearSearchCommand { get; }

        private void FilterUsers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Users = new ObservableCollection<User>(_allUsers);
            }
            else
            {
                Users = new ObservableCollection<User>(
                    _allUsers.Where(u => u.Username.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)));
            }
        }

        private void AddUser()
        {
            var newUser = new User();
            var window = new AddUserWindow(newUser, Roles);
            if (window.ShowDialog() == true)
            {
                _dbService.AddUser(newUser);
                _allUsers.Add(newUser);
                FilterUsers();
            }
        }

        private bool CanAddUser() => CurrentUser != null && PermissionChecker.CanManageUsers();

        private void EditUser()
        {
            if (SelectedUser == null) return;

            var userToUpdate = new User
            {
                UserId = SelectedUser.UserId,
                Username = SelectedUser.Username,
                Password = "", // Пустой пароль для UI
                RoleId = SelectedUser.RoleId
            };
            var window = new EditUserWindow(userToUpdate, Roles);
            if (window.ShowDialog() == true)
            {
                _dbService.UpdateUser(userToUpdate);
                var index = _allUsers.IndexOf(_allUsers.FirstOrDefault(u => u.UserId == userToUpdate.UserId));
                if (index != -1)
                {
                    _allUsers[index] = userToUpdate;
                    _allUsers[index].Password = SelectedUser.Password; // Восстанавливаем старый хэш после обновления
                }
                FilterUsers();
            }
        }

        private bool CanUpdateUser() => CurrentUser != null && PermissionChecker.CanManageUsers() && SelectedUser != null;

        private void DeleteUser()
        {
            if (SelectedUser == null) return;

            if (MessageBox.Show($"Удалить пользователя {SelectedUser.Username}?",
                "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _dbService.DeleteUser(SelectedUser.UserId);
                _allUsers.Remove(SelectedUser);
                FilterUsers();
            }
        }

        private bool CanDeleteUser() => CurrentUser != null && PermissionChecker.CanManageUsers() && SelectedUser != null;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}