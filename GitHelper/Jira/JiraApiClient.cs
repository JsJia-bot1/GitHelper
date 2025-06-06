using GitHelper.Common.Helper;
using Newtonsoft.Json;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace GitHelper.Jira
{
    public static class JiraApiClient
    {
        public static async Task<IReadOnlyCollection<JiraTicketModel>> GetAllTicketsByFixVersionAsync(string fixVersion)
        {
            using HttpClient client = CreateJiraApiClient();

            Dictionary<string, JiraTicketModel> ticketMap = [];

            int startAt = 0;
            const int maxResults = 50;
            bool hasMore = true;

            while (hasMore)
            {
                var jql = $"fixVersion = '{Uri.EscapeDataString(fixVersion)}' ORDER BY created DESC";

                var baseUrl = ConfigurationManager.AppSettings["JiraHost"];
                if (string.IsNullOrWhiteSpace(baseUrl))
                    throw new InvalidOperationException("Missing configuration: JiraHost");

                var url = $"{baseUrl.TrimEnd('/')}/rest/api/3/search?" +
                          $"jql={jql}&" +
                          $"startAt={startAt}&" +
                          $"maxResults={maxResults}&" +
                          "fields=key,summary,status,fixVersions,subtasks,issuelinks";

                var response = await client.GetAsync(url);
                ThrowHelper.ThrowHandledException(response.StatusCode == HttpStatusCode.BadRequest,
                    $"Cannot find tickets by version {fixVersion}, please double check.");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<JiraTicketsModel>(content)!;

                foreach (var issue in result.Issues)
                {
                    if (!ticketMap.ContainsKey(issue.Key))
                        ticketMap[issue.Key] = issue;

                    // Subtasks
                    if (issue.Fields.Subtasks != null)
                    {
                        foreach (var sub in issue.Fields.Subtasks)
                        {
                            if (!ticketMap.ContainsKey(sub.Key))
                            {
                                ticketMap[sub.Key] = new JiraTicketModel
                                {
                                    Key = sub.Key,
                                    Fields = new JiraTicketModel.FieldsData
                                    {
                                        Status = new JiraTicketModel.StatusData { Name = "(Sub-task)" }
                                    }
                                };
                            }
                        }
                    }

                    // Linked Issues
                    if (issue.Fields.IssueLinks != null)
                    {
                        foreach (var link in issue.Fields.IssueLinks)
                        {
                            var linkedKeys = new[] { link.InwardIssue?.Key, link.OutwardIssue?.Key };
                            foreach (var key in linkedKeys.Where(k => k != null))
                            {
                                if (!ticketMap.ContainsKey(key!))
                                {
                                    ticketMap[key!] = new JiraTicketModel
                                    {
                                        Key = key!,
                                        Fields = new JiraTicketModel.FieldsData
                                        {
                                            Status = new JiraTicketModel.StatusData { Name = "(Linked)" }
                                        }
                                    };
                                }
                            }
                        }
                    }
                }

                startAt += maxResults;
                hasMore = startAt < result.Total;
            }

            return [.. ticketMap.Values];
        }

        private static HttpClient CreateJiraApiClient()
        {
            HttpClient httpClient = new();
            var authValue = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{ConfigurationManager.AppSettings["JiraUser"]}:{ConfigurationManager.AppSettings["JiraToken"]}"));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", authValue);

            return httpClient;
        }
    }
}
