using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.States;

/// <summary>
/// Published, but the exam window has not opened yet. Only an Exams administrator may edit or
/// archive it; an author who is not an administrator is refused.
/// </summary>
public sealed class ScheduledState : ExamStateBase
{
    public override string Name => "scheduled";

    public override string DisplayName => "Scheduled";

    public override bool IsEditable => true;

    public override string? GetEditRefusal(ExamUser user)
    {
        if (RefuseNonAuthor(user) is { } refusal)
        {
            return refusal;
        }

        return user.MayManage ? null : Messages.ManageOnly;
    }

    public override bool CanArchive(ExamUser user) => user.MayManage;

    public override IExamState Archive() => new ArchivedState();
}
