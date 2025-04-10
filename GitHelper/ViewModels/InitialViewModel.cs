using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHelper.Common;
using GitHelper.Views;
using Microsoft.Win32;

namespace GitHelper.ViewModels
{
    public partial class InitialViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? _fixVersion;

        [RelayCommand]
        private async Task OpenGitDirectory()
        {
            OpenFileDialog dialog = new()
            {
                Filter = "gitignore|*.gitignore"
            };

            bool hasOpened = (bool)dialog.ShowDialog()!;
            if (!hasOpened)
            {
                return;
            }

            string repoPath = dialog.FileName[..dialog.FileName.LastIndexOf('\\')];

            await NavigationHelper.NavigateAsync<InitialPage, MainPage, MainViewModel>
                (async model => await model.Initialize(repoPath, FixVersion));
        }

    }
}
