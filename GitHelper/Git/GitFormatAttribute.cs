namespace GitHelper.Git
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class GitFormatAttribute(string val) : Attribute
    {
        public string Format { get; init; } = val;

    }
}
