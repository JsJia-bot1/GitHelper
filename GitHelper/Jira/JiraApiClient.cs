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
        public static async Task<IReadOnlyCollection<JiraTicketModel>> GetTicketsByFixVersionAsync(string fixVersion)
        {
            using HttpClient client = CreateJiraApiClient();

            List<JiraTicketModel> tickets = [];
            int startAt = 0;
            const int maxResults = 50;
            bool hasMore = true;

            while (hasMore)
            {
                var jql = $"fixVersion = '{Uri.EscapeDataString(fixVersion)}' ORDER BY created DESC";

                var url = $"{ConfigurationManager.AppSettings["JiraHost"]}/rest/api/3/search?" +
                          $"jql={jql}&" +
                          $"startAt={startAt}&" +
                          $"maxResults={maxResults}&" +
                          "fields=key,summary,status,fixVersions";

                var response = await client.GetAsync(url);

                ThrowHelper.ThrowHandledException(response.StatusCode == HttpStatusCode.BadRequest,
                                                  $"Cannot find tickets by version {fixVersion}, please double check.");

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<JiraTicketsModel>(content)!;

                tickets.AddRange(result.Issues);
                startAt += maxResults;
                hasMore = startAt < result.Total;
            }

            return tickets;
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
