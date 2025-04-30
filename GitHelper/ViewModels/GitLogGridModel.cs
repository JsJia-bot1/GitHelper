using CommunityToolkit.Mvvm.ComponentModel;
using GitHelper.Git;

namespace GitHelper.ViewModels
{
    public partial class GitLogGridModel(GitLogModel gitLog) : ObservableObject
    {
        [ObservableProperty]
        private bool _isChecked;

        public string? JiraNo { get; set; }

        public GitLogModel GitLog { get; init; } = gitLog;

        [ObservableProperty]
        private CherryPickStatus _status;

    }
}
