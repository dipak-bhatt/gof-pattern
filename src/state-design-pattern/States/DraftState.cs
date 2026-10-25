using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.States;

/// <summary>
/// Never published. An author may edit and delete it; an Exams administrator may publish it.
/// </summary>
public sealed class DraftState : ExamStateBase
{
    public override string Name => "draft";

    public override string DisplayName => "Draft";

    public override bool IsEditable => true;

    public override string? GetEditRefusal(ExamUser user) => RefuseNonAuthor(user);

    public override bool CanPublish(ExamUser user) => user.MayManage;

    public override bool CanDelete(ExamUser user) => user.MayAuthor;

    public override IExamState Publish() => new ScheduledState();

    public override IExamState Archive() =>
        throw new TransitionRefusedException(Messages.DraftCannotBeArchived);
}
