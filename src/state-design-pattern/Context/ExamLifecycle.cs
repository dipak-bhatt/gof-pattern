using StateDesignPattern.Models;
using StateDesignPattern.State;

namespace StateDesignPattern.Context;

/// <summary>
/// The Context. It holds the current state and forwards every question and transition to it.
/// The client talks to this object; the state classes never know about the client.
/// </summary>
public sealed class ExamLifecycle(string title, IExamState state)
{
    public string Title { get; } = title;

    public IExamState State { get; private set; } = state;

    public string? GetEditRefusal(ExamUser user) => State.GetEditRefusal(user);

    public bool CanPublish(ExamUser user) => State.CanPublish(user);

    public bool CanArchive(ExamUser user) => State.CanArchive(user);

    public bool CanDelete(ExamUser user) => State.CanDelete(user);

    public void Publish() => State = State.Publish();

    public void Archive() => State = State.Archive();

    public string Describe() =>
        $"{Title} | {State.DisplayName} [{State.Name}] | editable={State.IsEditable}";
}
