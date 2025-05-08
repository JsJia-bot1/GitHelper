namespace GitHelper.Git
{
    public enum CherryPickStatus
    {
        Ready,
        NoChange,
        DescriptionFound,
        CherryPicked,
        Conflicting,
        ConflictResolved
    }
}
