using GitHelper.Git;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GitHelper.ViewModels
{
    public class GitLogGridModel(GitLogModel gitLog) : INotifyPropertyChanged
    {
        private bool _isChecked;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnPropertyChanged(nameof(IsChecked));
                }
            }
        }

        public string? JiraNo { get; set; }

        public GitLogModel GitLog { get; init; } = gitLog;

        private CherryPickStatus _status;

        public CherryPickStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
