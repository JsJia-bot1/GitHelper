namespace GitHelper.Git
{
    public enum CherryPickStatus
    {
        Ready,
        NoChange,
        CherryPicked,
        Conflicting,
        ConflictResolved
    }
}
