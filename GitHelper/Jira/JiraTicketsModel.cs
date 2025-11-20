using Newtonsoft.Json;

namespace GitHelper.Jira
{
    public class JiraTicketsModel
    {
        public int Total { get; init; }

        [JsonProperty("nextPageToken")]
        public string? NextPageToken { get; set; }

        public IEnumerable<JiraTicketModel> Issues { get; init; } = null!;
    }

    public class JiraTicketModel
    {
        public string Key { get; init; } = null!;

        public FieldsData Fields { get; init; } = null!;

        public class FieldsData
        {
            public StatusData Status { get; init; } = null!;

            public List<SubTaskData>? Subtasks { get; init; }

            public List<IssueLinkData>? IssueLinks { get; init; }
        }

        public class StatusData
        {
            public string Name { get; init; } = null!;
        }

        public class SubTaskData
        {
            public string Key { get; init; } = null!;
        }

        public class IssueLinkData
        {
            public LinkedIssue? InwardIssue { get; init; }

            public LinkedIssue? OutwardIssue { get; init; }

            public class LinkedIssue
            {
                public string Key { get; init; } = null!;
            }
        }
    }
}
