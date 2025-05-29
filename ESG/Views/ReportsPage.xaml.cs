using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ESG.Views
{
    public partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            InitializeComponent();
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as ReportsViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = true;
                    if (!viewModel.SelectedCompanies.Contains(company))
                    {
                        viewModel.SelectedCompanies.Add(company);
                    }
                    viewModel.UpdateFilteredReports();
                }
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Company company)
            {
                var viewModel = DataContext as ReportsViewModel;
                if (viewModel != null)
                {
                    company.IsSelected = false;
                    viewModel.SelectedCompanies.Remove(company);
                    viewModel.UpdateFilteredReports();
                }
            }
        }

        private void ReportCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Report report)
            {
                var viewModel = DataContext as ReportsViewModel;
                if (viewModel != null)
                {
                    report.IsSelected = true;
                    if (!viewModel.SelectedReports.Contains(report))
                    {
                        viewModel.SelectedReports.Add(report);
                    }
                    viewModel.UpdateFilteredReports();
                }
            }
        }

        private void ReportCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Report report)
            {
                var viewModel = DataContext as ReportsViewModel;
                if (viewModel != null)
                {
                    report.IsSelected = false;
                    viewModel.SelectedReports.Remove(report);
                    viewModel.UpdateFilteredReports();
                }
            }
        }

        private void ChangeLog_Click(object sender, MouseButtonEventArgs e)
        {
            var viewModel = (ReportsViewModel)DataContext;
            viewModel.ExportChangeLog();
        }
    }
}