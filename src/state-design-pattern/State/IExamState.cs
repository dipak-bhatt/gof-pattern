using StateDesignPattern.Models;

namespace StateDesignPattern.State;

/// <summary>
/// The State contract. Each exam lifecycle state owns its own rules: who may edit it, which
/// operations are allowed, and what state comes next. The context and the client never switch
/// on a status value.
/// </summary>
public interface IExamState
{
    /// <summary>The machine-readable state name.</summary>
    string Name { get; }

    /// <summary>The human-readable label shown in the UI.</summary>
    string DisplayName { get; }

    /// <summary>Whether any edit is allowed in this state at all.</summary>
    bool IsEditable { get; }

    /// <summary>Why the user may not edit, or <c>null</c> when the edit is allowed.</summary>
    string? GetEditRefusal(ExamUser user);

    bool CanPublish(ExamUser user);

    bool CanArchive(ExamUser user);

    bool CanDelete(ExamUser user);

    /// <summary>Returns the next state after publishing.</summary>
    /// <exception cref="TransitionRefusedException">The state cannot be published.</exception>
    IExamState Publish();

    /// <summary>Returns the next state after archiving.</summary>
    /// <exception cref="TransitionRefusedException">The state cannot be archived.</exception>
    IExamState Archive();
}
