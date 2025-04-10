using GitHelper.ViewModels;
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
    }
}
