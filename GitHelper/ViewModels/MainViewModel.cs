using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GitHelper.Common;
using GitHelper.Common.Helper;
using GitHelper.Git;
using GitHelper.Jira;
using GitHelper.Views;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace GitHelper.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private GitCommandWrapper Git = null!;

        private IEnumerable<GitLogGridModel> _logs = [];

        [ObservableProperty]
        private ObservableCollection<GitLogGridModel> _displayedLogs = [];

        [ObservableProperty]
        private ObservableCollection<string> _jiraNos = [];

        [ObservableProperty]
        private string _repoPath = null!;

        [ObservableProperty]
        private string _currentBranch = null!;

        [ObservableProperty]
        private string _selectedJiraNo = "Jira No";

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private string? _targetBranch;

        public async Task Initialize(string repoPath, string? fixVersion = null)
        {
            Task<IReadOnlyCollection<JiraTicketModel>> jiraTask = InitializeJira(fixVersion);

            Task<IEnumerable<GitLogGridModel>> gitTask = Task.Run(() => InitializeGitRepo(repoPath));

            await Task.WhenAll(gitTask, jiraTask);

            IEnumerable<GitLogGridModel> logs = gitTask.Result;

            IReadOnlyCollection<JiraTicketModel> tickets = jiraTask.Result;

            FilterLogsByJiraNoAndRender(logs, tickets);
        }

        private IEnumerable<GitLogGridModel> InitializeGitRepo(string repoPath)
        {
            RepoPath = repoPath;

            Git = new(repoPath);
            Git.ValidateRepo();
            Git.CheckoutOrAutoCreate("dev", "origin/dev");
            Git.Pull("dev");

            return Git.Logs("dev").Select(x => new GitLogGridModel(x));
        }

        private static async Task<IReadOnlyCollection<JiraTicketModel>> InitializeJira(string? fixVersion)
        {
            if (string.IsNullOrWhiteSpace(fixVersion))
            {
                return [];
            }

            IReadOnlyCollection<JiraTicketModel> tickets = await JiraApiClient.GetTicketsByFixVersionAsync(fixVersion);
            return tickets;
        }

        private void FilterLogsByJiraNoAndRender(IEnumerable<GitLogGridModel> logs,
                                                 IReadOnlyCollection<JiraTicketModel> tickets)
        {
            if (tickets.Count != 0)
            {
                HashSet<string> jiraNos = [.. tickets.Select(x => x.Key)];

                logs = logs.Where(log =>
                {
                    string? jiraNo = jiraNos.FirstOrDefault(no => log.GitLog.Description.Contains(no));

                    if (jiraNo == null)
                    {
                        return false;
                    }

                    log.JiraNo = jiraNo;
                    return true;
                });

                JiraNos = new(jiraNos);
            }

            _logs = [.. logs];
            DisplayedLogs = new(_logs);
            CurrentBranch = "dev";
        }

        [RelayCommand]
        private static void GoBack()
        {
            NavigationHelper.GoBack<MainPage>();
        }

        [RelayCommand]
        private void CheckAll()
        {
            bool hasAllChecked = DisplayedLogs.All(item => item.IsChecked);
            foreach (var item in DisplayedLogs)
            {
                item.IsChecked = !hasAllChecked;
            }
        }

        [RelayCommand]
        private void Refresh()
        {
            SearchText = null;
            DisplayedLogs.UpdateAll(_logs);
        }

        [RelayCommand]
        private void Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayedLogs.UpdateAll(_logs);
                return;
            }

            DisplayedLogs.UpdateAll(_logs.Where(x => x.GitLog.Description.Contains(SearchText)
                                                  || x.GitLog.Author.Contains(SearchText)));
        }

        [RelayCommand]
        private void SelectJiraNo(string jiraNo)
        {
            DisplayedLogs.UpdateAll(_logs.Where(x => x.JiraNo == jiraNo));

            SelectedJiraNo = jiraNo;
        }

        [RelayCommand]
        private void Checkout()
        {
            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(TargetBranch), "Please input the target branch.");
            Debug.Assert(TargetBranch != null);

            bool autoCreate = Git.CheckoutOrAutoCreate(TargetBranch, "origin/main");

            MessageBoxHelper.ShowIfTrue(autoCreate, "The branch has been created automatically based on origin/main.");

            CurrentBranch = TargetBranch;
        }

        [RelayCommand]
        private void CherryPick()
        {
            ThrowHelper.ThrowHandledException(CurrentBranch == "dev", "Please checkout to new branch firstly.");

            IEnumerable<GitLogGridModel> checkedItems = DisplayedLogs.Where(x => x.IsChecked
                                                                              && x.Status == CherryPickStatus.Ready)
                                                                     .OrderBy(x => DateTime.Parse(x.GitLog.CommitDate));
            ThrowHelper.ThrowHandledException(!checkedItems.Any(), "No any ready items are checked.");

            foreach (var item in checkedItems)
            {
                CherryPick(item);
            }
        }

        private void CherryPick(GitLogGridModel log)
        {
            CherryPickStatus status = Git.CherryPick(log.GitLog.Hash);

            if (status != CherryPickStatus.Conflicting)
            {
                log.Status = status;
                return;
            }

            ResolveConflict(log);

            // Ensure no abort
            if (!Git.Contains(log.GitLog.Description, CurrentBranch))
            {
                CherryPick(log);
            }

            log.Status = CherryPickStatus.ConflictResolved;
        }

        private void ResolveConflict(GitLogGridModel log)
        {
            do
            {
                MessageBox.Show($"The commit {log.GitLog.Description} occurs conflict, please resolve.");
            }
            while (Git.HasUnresolvedConflict());
        }
    }
}
