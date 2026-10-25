using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.States;

/// <summary>The final state: no edit, publish, archive or delete is allowed.</summary>
public sealed class ArchivedState : ExamStateBase
{
    public override string Name => "archived";

    public override string DisplayName => "Archived";

    public override bool IsEditable => false;

    public override string? GetEditRefusal(ExamUser user) =>
        RefuseNonAuthor(user) ?? Messages.AlreadyArchived;
}
