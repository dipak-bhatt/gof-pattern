namespace StateDesignPattern.State;

/// <summary>User-facing refusal messages, kept in one place so every state speaks consistently.</summary>
internal static class Messages
{
    public const string AccessDenied =
        "Only an exam author or an Exams administrator can do this.";

    public const string ManageOnly =
        "Only an Exams administrator can publish, archive or change a scheduled exam.";

    public const string LiveAndLocked =
        "The exam is live and locked.";

    public const string CompletedAndLocked =
        "The exam is completed and locked.";

    public const string AlreadyArchived =
        "The exam is archived.";

    public const string OnlyDraftCanBePublished =
        "Only a draft can be published.";

    public const string DraftCannotBeArchived =
        "A draft is deleted, not archived.";

    public const string LiveCannotBeArchived =
        "Wait for the exam to complete, or close it early.";
}
