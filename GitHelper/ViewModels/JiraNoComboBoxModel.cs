using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace GitHelper.ViewModels
{
    public partial class JiraNoComboBoxModel(string jiraNo, ICommand checkJiraNoCommand) : ObservableObject
    {
        public string JiraNo { get; init; } = jiraNo;

        [ObservableProperty]
        private bool _isChecked;

        public ICommand CheckJiraNoCommand { get; init; } = checkJiraNoCommand;
    }
}
