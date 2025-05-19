using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class NewsPage : Page
    {
        public NewsPage()
        {
            InitializeComponent();
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as NewsViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = true;
                    if (!viewModel.SelectedCompanies.Contains(company))
                    {
                        viewModel.SelectedCompanies.Add(company);
                    }
                    viewModel.UpdateFilteredNews();
                    viewModel.UpdateFilteredNewsForList();
                }
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as NewsViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = false;
                    viewModel.SelectedCompanies.Remove(company);
                    viewModel.UpdateFilteredNews();
                    viewModel.UpdateFilteredNewsForList();
                }
            }
        }

        private void NewsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is News news)
            {
                var viewModel = DataContext as NewsViewModel;
                if (viewModel != null)
                {
                    news.IsSelected = true;
                    if (!viewModel.SelectedNews.Contains(news))
                    {
                        viewModel.SelectedNews.Add(news);
                    }
                    viewModel.UpdateFilteredNews();
                }
            }
        }

        private void NewsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is News news)
            {
                var viewModel = DataContext as NewsViewModel;
                if (viewModel != null)
                {
                    news.IsSelected = false;
                    viewModel.SelectedNews.Remove(news);
                    viewModel.UpdateFilteredNews();
                }
            }
        }
    }
}