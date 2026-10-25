using StateDesignPattern.Models;

namespace StateDesignPattern.State;

/// <summary>
/// Shared defaults for all states: the author gate that runs before any state-specific rule,
/// and a safe "no" for capabilities and transitions that a state does not allow.
/// </summary>
public abstract class ExamStateBase : IExamState
{
    public abstract string Name { get; }

    public abstract string DisplayName { get; }

    public abstract bool IsEditable { get; }

    public abstract string? GetEditRefusal(ExamUser user);

    public virtual bool CanPublish(ExamUser user) => false;

    public virtual bool CanArchive(ExamUser user) => false;

    public virtual bool CanDelete(ExamUser user) => false;

    public virtual IExamState Publish() =>
        throw new TransitionRefusedException(Messages.OnlyDraftCanBePublished);

    public virtual IExamState Archive() =>
        throw new TransitionRefusedException(Messages.AlreadyArchived);

    /// <summary>Everyone who edits or deletes must first be an author.</summary>
    protected static string? RefuseNonAuthor(ExamUser user) =>
        user.MayAuthor ? null : Messages.AccessDenied;
}
