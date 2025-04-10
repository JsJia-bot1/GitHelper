namespace GitHelper.Jira
{
    public class JiraTicketsModel
    {
        public int Total { get; init; }

        public IEnumerable<JiraTicketModel> Issues { get; init; } = null!;
    }

    public class JiraTicketModel
    {
        public string Key { get; init; } = null!;

        public FieldsData Fields { get; init; } = null!;

        public class FieldsData
        {
            public StatusData Status { get; init; } = null!;
        }

        public class StatusData
        {
            public string Name { get; init; } = null!;
        }

    }
}
