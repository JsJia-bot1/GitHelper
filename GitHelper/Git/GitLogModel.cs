using System;
using System.Reflection;
using System.Text;

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

            StringBuilder builder = new("{");

            var entries = properties.Select(p =>
            {
                var attribute = p.GetCustomAttribute<GitFormatAttribute>()!;
                return $"\"\"\"{p.Name}\"\"\":\"\\\"{attribute.Format}\\\"\"";
            });

            builder.Append(string.Join(",", entries));
            builder.Append('}');

            return builder.ToString();
        }
    }
}
