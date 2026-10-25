using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.States;

/// <summary>
/// The exam window is open. The exam is locked for everyone and cannot be archived until it
/// completes.
/// </summary>
public sealed class LiveState : ExamStateBase
{
    public override string Name => "live";

    public override string DisplayName => "Live";

    public override bool IsEditable => false;

    public override string? GetEditRefusal(ExamUser user) =>
        RefuseNonAuthor(user) ?? Messages.LiveAndLocked;

    public override IExamState Archive() =>
        throw new TransitionRefusedException(Messages.LiveCannotBeArchived);
}
