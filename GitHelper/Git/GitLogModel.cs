using System.Reflection;

namespace GitHelper.Git
{
    public class GitLogModel
    {
        [GitFormat("%h")]
        public string Hash { get; init; } = null!;

        [GitFormat("%an")]
        public string Author { get; init; } = null!;

        [GitFormat("%cd")]
        public DateTime CommitDate { get; init; }

        [GitFormat("%s")]
        public string Description { get; init; } = null!;

        public static string Formatter()
        {
            var properties = typeof(GitLogModel)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.IsDefined(typeof(GitFormatAttribute)));

            var entries = properties.Select(p =>
            {
                var attribute = p.GetCustomAttribute<GitFormatAttribute>()!;
                return $"\"\"\"x00{p.Name}\"\"\"x00:\"\\\"x00{attribute.Format}\\\"\"x00";
            });

            return $@"{{{string.Join(",", entries)}}}";
        }
    }
}
