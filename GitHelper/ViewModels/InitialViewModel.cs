using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHelper.Common;
using GitHelper.Common.Helper;
using GitHelper.Views;
using Microsoft.Win32;

namespace GitHelper.ViewModels
{
    public partial class InitialViewModel : ObservableObject
    {
        [ObservableProperty]
        private InitialConfigModel _configModel = null!;

        public InitialViewModel()
        {
            ConfigModel = LocalCacheHelper.GetCache<InitialConfigModel>("Config") ?? new();
        }

        [RelayCommand]
        private void OpenGitDirectory()
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

            ConfigModel.RepoPath = repoPath;
        }

        [RelayCommand]
        private async Task Start()
        {
            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(ConfigModel.RepoPath), "Please select a Git repository.");

            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(ConfigModel.SourceBranch), "Please input the source branch.");

            _ = LocalCacheHelper.SaveCacheAsync(ConfigModel, "Config");

            await NavigationHelper.NavigateAsync<InitialPage, MainPage, MainViewModel>
                (async model => await model.Initialize(ConfigModel));

        }
    }
}
