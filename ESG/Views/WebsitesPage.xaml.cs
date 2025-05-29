using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESG.Views
{
    public partial class WebsitesPage : Page
    {
        public WebsitesPage()
        {
            InitializeComponent();
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as WebsitesViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = true;
                    if (!viewModel.SelectedCompanies.Contains(company))
                    {
                        viewModel.SelectedCompanies.Add(company);
                    }
                    viewModel.UpdateFilteredWebsites();
                    viewModel.UpdateFilteredWebsitesForList(); // Обновляем второй фильтр
                }
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as WebsitesViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = false;
                    viewModel.SelectedCompanies.Remove(company);
                    viewModel.UpdateFilteredWebsites();
                    viewModel.UpdateFilteredWebsitesForList(); // Обновляем второй фильтр
                }
            }
        }

        private void WebsiteCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Website website)
            {
                var viewModel = DataContext as WebsitesViewModel;
                if (viewModel != null)
                {
                    website.IsSelected = true;
                    if (!viewModel.SelectedWebsites.Contains(website))
                    {
                        viewModel.SelectedWebsites.Add(website);
                    }
                    viewModel.UpdateFilteredWebsites();
                }
            }
        }

        private void WebsiteCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Website website)
            {
                var viewModel = DataContext as WebsitesViewModel;
                if (viewModel != null)
                {
                    website.IsSelected = false;
                    viewModel.SelectedWebsites.Remove(website);
                    viewModel.UpdateFilteredWebsites();
                }
            }
        }

        private void ChangeLog_Click(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (WebsitesViewModel)DataContext;
            viewModel.ExportChangeLog();
        }
    }
}