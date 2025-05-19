using ESG.Models;
using ESG.ViewModels;
using System.Windows;

namespace ESG.Views
{
    public partial class AddIndustryWindow : Window
    {
        private string _industryName;
        private readonly Industry _industry;
        private readonly IndustriesViewModel _viewModel;

        public string IndustryName
        {
            get => _industryName;
            set
            {
                _industryName = value;
                OnPropertyChanged(); // Убедитесь, что INotifyPropertyChanged реализован
            }
        }

        public AddIndustryWindow(Industry industry, IndustriesViewModel viewModel)
        {
            InitializeComponent();
            _industry = industry;
            _viewModel = viewModel;
            _industryName = industry?.IndustryName ?? string.Empty;
            Title = industry == null ? "Добавить отрасль" : "Редактировать отрасль";
            DataContext = this;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_industryName))
            {
                MessageBox.Show("Пожалуйста, укажите название отрасли.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_industry != null)
            {
                _industry.IndustryName = _industryName; // Обновляем объект отрасли
            }
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}