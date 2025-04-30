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
        private ObservableCollection<JiraNoComboBoxModel> _jiraNos = [];

        [ObservableProperty]
        private string _repoPath = null!;

        [ObservableProperty]
        private string _currentBranch = null!;

        [ObservableProperty]
        private string? _selectedJiraNosText;

        [ObservableProperty]
        private string? _searchText;

        [ObservableProperty]
        private string? _targetBranch;

        private const string DROPDOWN_SELECTALL_TEXT = "Select All";

        public async Task Initialize(string repoPath, string? fixVersion = null)
        {
            Task<IReadOnlyCollection<JiraTicketModel>> jiraTask = InitializeJira(fixVersion);

            Task<IEnumerable<GitLogGridModel>> gitTask = InitializeGitRepo(repoPath);

            await Task.WhenAll(gitTask, jiraTask);

            IEnumerable<GitLogGridModel> logs = gitTask.Result;

            IReadOnlyCollection<JiraTicketModel> tickets = jiraTask.Result;

            FilterLogsByJiraNoAndRender(logs, tickets);
        }

        private async Task<IEnumerable<GitLogGridModel>> InitializeGitRepo(string repoPath)
        {
            RepoPath = repoPath;

            Git = new(repoPath);
            await Git.ValidateRepo();
            await Git.CheckoutOrAutoCreate("dev", "origin/dev");
            await Git.Pull("dev");

            return (await Git.Logs("dev")).Select(x => new GitLogGridModel(x));
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

                logs = [.. logs.Where(log =>
                {
                    string? jiraNo = jiraNos.FirstOrDefault(no => log.GitLog.Description.Contains(no));

                    if (jiraNo == null)
                    {
                        return false;
                    }

                    log.JiraNo = jiraNo;
                    return true;
                })];

                JiraNos = new(jiraNos.Intersect(logs.Select(x => x.JiraNo))
                                     .OrderBy(x => x)
                                     .Select(x => new JiraNoComboBoxModel(x!, CheckJiraNoCommand)));

                JiraNos.Insert(0, new JiraNoComboBoxModel(DROPDOWN_SELECTALL_TEXT, CheckJiraNoCommand));
            }

            _logs = logs;
            DisplayedLogs = new(_logs);
            CurrentBranch = "dev";
        }

        [RelayCommand]
        private static void GoBack()
        {
            NavigationHelper.GoBack<MainPage>();
        }

        [RelayCommand]
        private void CheckAll(bool isChecked)
        {
            foreach (var item in DisplayedLogs)
            {
                item.IsChecked = isChecked;
            }
        }

        [RelayCommand]
        private void CheckJiraNo(JiraNoComboBoxModel model)
        {
            // Handle check all
            if (model.JiraNo == DROPDOWN_SELECTALL_TEXT)
            {
                foreach (var item in JiraNos.Skip(1))
                {
                    item.IsChecked = model.IsChecked;
                }

                SelectedJiraNosText = model.IsChecked ? string.Join(", ", JiraNos.Skip(1).Select(x => x.JiraNo))
                                                      : string.Empty;
                return;
            }

            SelectedJiraNosText = string.Join(", ", JiraNos.Skip(1).Where(x => x.IsChecked).Select(x => x.JiraNo));
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
            var checkedJiraNos = JiraNos.Where(x => x.IsChecked).Select(x => x.JiraNo);

            if (string.IsNullOrWhiteSpace(SearchText) && !checkedJiraNos.Any())
            {
                DisplayedLogs.UpdateAll(_logs);
                return;
            }

            IList<Func<GitLogGridModel, bool>>? filters = [];
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filters.Add(x => x.GitLog.Description.Contains(SearchText) || x.GitLog.Author.Contains(SearchText));
            }

            if (checkedJiraNos.Any())
            {
                filters.Add(x => checkedJiraNos.Contains(x.JiraNo));
            }

            DisplayedLogs.UpdateAll(_logs.Where(x => filters.All(f => f.Invoke(x))));
        }


        [RelayCommand]
        private async Task Checkout()
        {
            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(TargetBranch), "Please input the target branch.");
            Debug.Assert(TargetBranch != null);

            bool autoCreate = await Git.CheckoutOrAutoCreate(TargetBranch, "origin/main");

            MessageBoxHelper.ShowIfTrue(autoCreate, "The branch has been created automatically based on origin/main.");

            CurrentBranch = TargetBranch;
        }

        [RelayCommand]
        private async Task CherryPick()
        {
            ThrowHelper.ThrowHandledException(CurrentBranch == "dev", "Please checkout to new branch firstly.");

            IEnumerable<GitLogGridModel> checkedItems = DisplayedLogs.Where(x => x.IsChecked
                                                                              && x.Status == CherryPickStatus.Ready)
                                                                     .OrderBy(x => DateTime.Parse(x.GitLog.CommitDate));
            ThrowHelper.ThrowHandledException(!checkedItems.Any(), "No any ready items are checked.");

            foreach (var item in checkedItems)
            {
                await CherryPick(item);
            }

            MessageBox.Show($"Total {checkedItems.Count()} items have been cherry-picked successfully.");
        }

        private async Task CherryPick(GitLogGridModel log)
        {
            CherryPickStatus status = await Git.CherryPick(log.GitLog.Hash);

            if (status != CherryPickStatus.Conflicting)
            {
                log.Status = status;
                return;
            }

            await ResolveConflict(log);

            // Ensure no abort
            if (!await Git.Contains(log.GitLog.Description, CurrentBranch))
            {
                await CherryPick(log);
            }

            log.Status = CherryPickStatus.ConflictResolved;
        }

        private async Task ResolveConflict(GitLogGridModel log)
        {
            do
            {
                MessageBox.Show($"The commit {log.GitLog.Description} occurs conflict, please resolve.");
            }
            while (await Git.HasUnresolvedConflict());
        }
    }
}
