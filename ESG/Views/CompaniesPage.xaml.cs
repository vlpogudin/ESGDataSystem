using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class CompaniesPage : Page
    {
        public CompaniesPage()
        {
            InitializeComponent();
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as CompaniesViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = true;
                    if (!viewModel.SelectedCompanies.Contains(company))
                    {
                        viewModel.SelectedCompanies.Add(company);
                    }
                    viewModel.UpdateFilteredCompanies();
                }
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as CompaniesViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = false;
                    viewModel.SelectedCompanies.Remove(company);
                    viewModel.UpdateFilteredCompanies();
                }
            }
        }
    }
}