using CommunityToolkit.Mvvm.ComponentModel;

namespace GitHelper.ViewModels
{
    public partial class InitialConfigModel : ObservableObject
    {
        [ObservableProperty]
        private string? _fixVersion;

        [ObservableProperty]
        private string? _repoPath;

        [ObservableProperty]
        private string _sourceBranch = "dev";
    }
}
