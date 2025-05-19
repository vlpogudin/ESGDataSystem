using System.Windows.Controls;
using System.Windows;
using ESG.Models;
using ESG.ViewModels;

namespace ESG.Views
{
    public partial class ExportDataPage : Page
    {
        public ExportDataPage()
        {
            InitializeComponent();
        }

        private void IndustryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var industry = checkBox.Content.ToString();
            if (!viewModel.SelectedIndustries.Contains(industry))
            {
                viewModel.SelectedIndustries.Add(industry);
            }
        }

        private void IndustryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var industry = checkBox.Content.ToString();
            viewModel.SelectedIndustries.Remove(industry);
        }

        private void CompanyCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var company = (Company)((FrameworkElement)sender).DataContext;
            if (!viewModel.SelectedCompanies.Contains(company))
            {
                viewModel.SelectedCompanies.Add(company);
                company.IsSelected = true;
            }
        }

        private void CompanyCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var company = (Company)((FrameworkElement)sender).DataContext;
            viewModel.SelectedCompanies.Remove(company);
            company.IsSelected = false;
        }

        private void ReportCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var report = checkBox.Content.ToString();
            if (!viewModel.SelectedReports.Contains(report))
            {
                viewModel.SelectedReports.Add(report);
            }
        }

        private void ReportCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var report = checkBox.Content.ToString();
            viewModel.SelectedReports.Remove(report);
        }

        private void NewsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var news = checkBox.Content.ToString();
            if (!viewModel.SelectedNews.Contains(news))
            {
                viewModel.SelectedNews.Add(news);
            }
        }

        private void NewsCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            var viewModel = (ExportDataViewModel)DataContext;
            var news = checkBox.Content.ToString();
            viewModel.SelectedNews.Remove(news);
        }
    }
}