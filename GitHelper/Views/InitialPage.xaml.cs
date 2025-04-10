using GitHelper.ViewModels;
using System.Windows.Controls;

namespace GitHelper.Views
{
    public partial class InitialPage : Page
    {
        public InitialPage(InitialViewModel model)
        {
            base.DataContext = model;
            InitializeComponent();
        }
    }
}