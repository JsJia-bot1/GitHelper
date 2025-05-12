using GitHelper.Common.Helper;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace GitHelper.Git
{
    public class GitCommandWrapper(string _directory)
    {

        private async Task<string> Execute(string command, string? errMessage = null)
        {
            ProcessStartInfo info = new("git", command)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                WorkingDirectory = _directory,
            };
            Process process = new()
            {
                StartInfo = info,
            };
            process.Start();

            string res = await process.StandardOutput.ReadToEndAsync();

            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(res) && !string.IsNullOrWhiteSpace(errMessage), errMessage);

            return res;
        }

        public async Task ValidateRepo()
        {
            await Execute("remote", "The selected path is not a valid Git repository.");
        }

        public async Task Checkout(string branchName)
        {
            await Execute($"checkout {branchName} --force");
        }

        public async Task Pull(string branchName)
        {
            await Execute($"pull origin {branchName}:{branchName}", $"Branch {branchName} not found, please check again.");
        }

        public async Task<bool> CheckoutOrAutoCreate(string branchName, string sourceBranch)
        {
            if (await IsBranchExisting(branchName))
            {
                await Checkout(branchName);
                return false;
            }

            await CreateBranch(branchName, sourceBranch);
            return true;
        }

        public async Task CreateBranch(string branchName, string sourceBranchName)
        {
            await Execute($"checkout -b {branchName} {sourceBranchName}",
                    $"Source branch {sourceBranchName} not found, please check again.");
        }

        public async Task<bool> IsBranchExisting(string branchName)
        {
            string res = await Execute($"show-branch {branchName}");
            return !string.IsNullOrEmpty(res);
        }

        public async Task<IEnumerable<GitLogModel>> Logs(string branchName)
        {
            string command = $@"log {branchName} --since=12.months --date=format-local:""%Y-%m-%d %H:%M:%S"" --pretty=format:""{GitLogModel.Formatter()},""";
            string sLogs = await Execute(command);
            sLogs = $@"[{sLogs.Remove(sLogs.Length - 1)}]";

            sLogs = sLogs.Replace("\"", "\\\"").Replace("\\\"x00", "\"");

            return JsonConvert.DeserializeObject<IEnumerable<GitLogModel>>(sLogs)!;
        }

        /// <summary>
        /// Check whether the commit exists on the branch
        /// </summary>
        /// <param name="commitMessage"></param>
        /// <param name="branchName"></param>
        /// <returns></returns>
        public async Task<bool> Contains(string commitMessage, string branchName)
        {
            string command = $@"log {branchName} --grep ""{commitMessage}""";
            string res = await Execute(command);

            return !string.IsNullOrEmpty(res);
        }

        public async Task<CherryPickStatus> CherryPick(string commitHash)
        {
            string res = await Execute($"cherry-pick {commitHash} -m 1");

            ThrowHelper.ThrowHandledException(string.IsNullOrWhiteSpace(res),
                                              "Unhandled exception occurred while cherry-pick, please retry.");

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

        public async Task<bool> HasUnresolvedConflict()
        {
            string res = await Execute($"status --porcelain");

            // Match the conflict mark  e.g. UU AA DD
            return Regex.IsMatch(res, @"^([A-Z]{2})\s", RegexOptions.Multiline);
        }
    }
}
