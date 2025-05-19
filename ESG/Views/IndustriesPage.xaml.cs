using ESG.Models;
using ESG.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace ESG.Views
{
    public partial class IndustriesPage : Page
    {
        public IndustriesPage()
        {
            InitializeComponent();
        }

        private void IndustryCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Models.IndustryItem industry)
            {
                var viewModel = DataContext as IndustriesViewModel;
                if (viewModel != null)
                {
                    industry.IsSelected = true;
                    if (!viewModel.SelectedIndustries.Contains(industry))
                    {
                        viewModel.SelectedIndustries.Add(industry);
                    }
                    viewModel.UpdateFilteredIndustries();
                }
            }
        }

        private void IndustryCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is Models.IndustryItem industry)
            {
                var viewModel = DataContext as IndustriesViewModel;
                if (viewModel != null)
                {
                    industry.IsSelected = false;
                    viewModel.SelectedIndustries.Remove(industry);
                    viewModel.UpdateFilteredIndustries();
                }
            }
        }
    }
}