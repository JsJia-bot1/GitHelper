using GitHelper.Common.Helper;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitHelper.Git
{
    public class GitCommandWrapper(string directory)
    {
        public string Directory { get; } = directory;

        private string Execute(string command, string? errMessage = null)
        {
            ProcessStartInfo info = new("git", command)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = Directory,
            };
            Process process = new()
            {
                StartInfo = info,
            };
            process.Start();

            string res = process.StandardOutput.ReadToEnd();

            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(res) && !string.IsNullOrWhiteSpace(errMessage), errMessage);

            return res;
        }

        public void ValidateRepo()
        {
            Execute("remote", "The selected path is not a valid Git repository.");
        }

        public void Checkout(string branchName)
        {
            Execute($"checkout {branchName} --force");
        }

        public void Pull(string branchName)
        {
            Execute($"pull origin {branchName}:{branchName}", $"Branch {branchName} not found, please check again.");
        }

        public bool CheckoutOrAutoCreate(string branchName, string sourceBranch)
        {
            if (IsBranchExisting(branchName))
            {
                Checkout(branchName);
                return false;
            }

            CreateBranch(branchName, sourceBranch);
            return true;
        }

        public void CreateBranch(string branchName, string sourceBranchName)
        {
            Execute($"checkout -b {branchName} {sourceBranchName}",
                    $"Source branch {sourceBranchName} not found, please check again.");
        }

        public bool IsBranchExisting(string branchName)
        {
            string res = Execute($"show-branch {branchName}");
            return !string.IsNullOrEmpty(res);
        }

        public IEnumerable<GitLogModel> Logs(string branchName)
        {
            string command = $@"log {branchName} --since=6.months --date=format:""%Y-%m-%d %H:%M:%S"" --pretty=format:""{GitLogModel.Formatter()},""";
            string sLogs = Execute(command);

            sLogs = sLogs.Remove(sLogs.Length - 1);
            sLogs = $@"[ {sLogs} ]";

            IEnumerable<GitLogModel> logs;
            try
            {
                logs = JsonConvert.DeserializeObject<IEnumerable<GitLogModel>>(sLogs)!;
            }
            catch (JsonReaderException) 
            {
                sLogs = sLogs.Replace(@"Reapply """, "Reapply \\\"");
                sLogs = sLogs.Replace(@"Revert """, "Revert \\\"");
                sLogs = sLogs.Replace(@")""""", ")\\\"\"");
                logs = JsonConvert.DeserializeObject<IEnumerable<GitLogModel>>(sLogs)!;
            }
            return logs;
        }

        /// <summary>
        /// Check whether the commit exists on the branch
        /// </summary>
        /// <param name="commitMessage"></param>
        /// <param name="branchName"></param>
        /// <returns></returns>
        public bool Contains(string commitMessage, string branchName)
        {
            string command = $@"log {branchName} --grep ""{commitMessage}""";
            string res = Execute(command);

            return !string.IsNullOrEmpty(res);
        }

        public CherryPickStatus CherryPick(string commitHash)
        {
            string res = Execute($"cherry-pick {commitHash} -m 1");

            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(res),
                                              "Unhandled exception occurrs whild cherry-pick, please retry.");

            if (res.StartsWith("CONFLICT") 
             || res.StartsWith("warning: Cannot merge binary files") 
             || res.Contains("CONFLICT (content): Merge conflict"))
            {
                return CherryPickStatus.Conflicting;
            }

            if (res.Contains("Your branch is ahead of"))
            {
                return CherryPickStatus.NoChange;
            }

            return CherryPickStatus.CherryPicked;
        }

        public bool HasUnresolvedConflict()
        {
            string res = Execute($"status --porcelain");

            // Match the conflict mark  e.g. UU AA DD
            return Regex.IsMatch(res, @"^([A-Z]{2})\s", RegexOptions.Multiline);
        }
    }
}
