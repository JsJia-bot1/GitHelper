using GitHelper.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace GitHelper.Views
{
    public partial class MainPage : Page
    {
        public MainPage(MainViewModel model)
        {
            base.DataContext = model;
            InitializeComponent();
        }

        private void SelectAll_CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = (MainViewModel)base.DataContext;
            mainViewModel.CheckAllCommand.Execute(true);
        }

        private void SelectAll_CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MainViewModel mainViewModel = (MainViewModel)base.DataContext;
            mainViewModel.CheckAllCommand.Execute(false);
        }
    }
}
