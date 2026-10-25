using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.States;

/// <summary>
/// The exam window has closed. The exam is locked, but an Exams administrator may archive it.
/// </summary>
public sealed class CompletedState : ExamStateBase
{
    public override string Name => "completed";

    public override string DisplayName => "Completed";

    public override bool IsEditable => false;

    public override string? GetEditRefusal(ExamUser user) =>
        RefuseNonAuthor(user) ?? Messages.CompletedAndLocked;

    public override bool CanArchive(ExamUser user) => user.MayManage;

    public override IExamState Archive() => new ArchivedState();
}
