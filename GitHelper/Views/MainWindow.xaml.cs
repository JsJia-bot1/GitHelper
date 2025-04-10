using MahApps.Metro.Controls;

namespace GitHelper.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow(InitialPage page)
        {
            InitializeComponent();
            MainFrame.Navigate(page);
        }
    }
}