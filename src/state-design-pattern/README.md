# State Design Pattern

## Definition

The State pattern **lets an object change its behaviour when its internal state changes**. Instead
of testing a status field with `if` or `switch` in every method, each state becomes its own class
with the same interface. The context holds one state object and forwards every request to it.

The object then appears to change its class at runtime: a draft exam and a live exam answer the
same methods, but a draft allows edits and a live exam refuses them.

This example models the lifecycle of an **exam**:

```text
draft ──publish──> scheduled ──(window opens)──> live ──(window closes)──> completed ──archive──> archived
                                                        │
                                                        └──(close early)──> completed
```

The rules are not the same in every state:

| State | May edit | May publish | May archive | May delete |
|-------|----------|-------------|-------------|------------|
| `draft` | author | Exams admin | no | author |
| `scheduled` | Exams admin | no | Exams admin | no |
| `live` | no one | no | no | no |
| `completed` | no one | no | Exams admin | no |
| `archived` | no one | no | no | no |

## The Problem

A status field plus `switch` starts simple:

```csharp
public string? GetEditRefusal(ExamUser user, string state)
{
    if (!user.MayAuthor)
    {
        return "Only an exam author or an Exams administrator can do this.";
    }

    return state switch
    {
        "draft" => null,
        "scheduled" => user.MayManage ? null : "Only an Exams administrator can change a scheduled exam.",
        "live" => "The exam is live and locked.",
        "completed" => "The exam is completed and locked.",
        "archived" => "The exam is archived.",
        _ => "That exam no longer exists.",
    };
}
```

Then the same state test is repeated for every capability:

```csharp
public static bool CanPublish(ExamUser user, string state) =>
    user.MayManage && state == "draft";

public static bool CanArchive(ExamUser user, string state) =>
    user.MayManage && state is "scheduled" or "completed";

public static bool CanDelete(ExamUser user, string state) =>
    user.MayAuthor && state == "draft";
```

Adding a state, or changing one rule, means finding and editing every switch. The rules for one
state are scattered across several methods.

With State, each state class owns all of its rules:

```csharp
public sealed class DraftState : ExamStateBase
{
    public override bool CanPublish(ExamUser user) => user.MayManage;

    public override bool CanDelete(ExamUser user) => user.MayAuthor;

    public override IExamState Publish() => new ScheduledState();
}
```

The client asks the context. The context asks the current state. No caller tests the status.

## Structure

```text
                     Client
                   Program.cs
                        │
                        │ GetEditRefusal / CanPublish / Publish / Archive
                        ▼
              ┌───────────────────┐
              │ CONTEXT           │
              │ ExamLifecycle     │
              │ IExamState State  │
              └─────────┬─────────┘
                        │ forwards every call
                        ▼
              ┌───────────────────┐
              │ STATE             │
              │ IExamState        │
              └─────────┬─────────┘
                        │ implemented by
     ┌────────────┬─────┴──────┬──────────────┬──────────────┐
     ▼            ▼            ▼              ▼              ▼
 ┌────────┐  ┌──────────┐  ┌────────┐  ┌───────────┐  ┌──────────┐
 │ Draft  │  │ Scheduled│  │  Live  │  │ Completed │  │ Archived │
 └───┬────┘  └────┬─────┘  └────────┘  └─────┬─────┘  └──────────┘
     │            │                          │
     │ Publish    │ Archive                  │ Archive
     └───────────>│─────────────────────────>│────────────────> Archived
```

The context does not know which concrete state it holds. The states do not know the context or the
client. Adding a new state means adding one class and wiring its transition; no existing state or
caller changes.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Context** | `ExamLifecycle` | Holds the current state and forwards requests to it |
| **State** | `IExamState` | Declares the operations whose behaviour depends on the state |
| **Abstract state** | `ExamStateBase` | Shares the author gate and default "not allowed" answers |
| **Concrete states** | `DraftState`, `ScheduledState`, `LiveState`, `CompletedState`, `ArchivedState` | Each owns one state's rules and its next state |
| **Data** | `ExamUser` | The caller's capabilities, kept free of framework concerns |
| **Client** | `Program.cs` | Creates the context and drives the lifecycle |

## Project Layout

```text
state-design-pattern/
├── Context/
│   └── ExamLifecycle.cs                  — CONTEXT: holds and forwards to the current state
├── Models/
│   ├── ExamUser.cs                       — the caller's capabilities
│   └── TransitionRefusedException.cs     — an illegal transition
├── State/
│   ├── IExamState.cs                     — STATE contract
│   ├── ExamStateBase.cs                  — shared author gate and safe defaults
│   └── Messages.cs                       — user-facing refusal messages
├── States/
│   ├── DraftState.cs                     — editable, deletable, publishable
│   ├── ScheduledState.cs                 — editable and archivable by an admin
│   ├── LiveState.cs                      — locked while the window is open
│   ├── CompletedState.cs                 — locked, archivable by an admin
│   └── ArchivedState.cs                  — final state
├── Program.cs                            — CLIENT and demonstration
├── state-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/state-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== State Design Pattern — Exam Lifecycle ===

CONTEXT   ExamLifecycle  holds the current state and forwards every call
STATE     IExamState     one object per lifecycle state
CONCRETE  Draft · Scheduled · Live · Completed · Archived

--- 1. Who may edit? Each state answers for itself. ---
  State        Author                                             Admin
  draft        Allowed                                            Allowed
  scheduled    Only an Exams administrator can publish, archive or change a scheduled exam. Allowed
  live         The exam is live and locked.                       The exam is live and locked.
  completed    The exam is completed and locked.                  The exam is completed and locked.
  archived     The exam is archived.                              The exam is archived.

--- 2. What may be done? Capabilities also belong to the state. ---
  State        Publish    Archive    Delete   (as an Exams admin)
  draft        True       False      True
  scheduled    False      True       False
  live         False      False      False
  completed    False      True       False
  archived     False      False      False

--- 3. A draft moves through the lifecycle; the context never switches. ---
  Created      Safety induction | Draft [draft] | editable=True
  Published    Safety induction | Scheduled [scheduled] | editable=True
  Archived     Safety induction | Archived [archived] | editable=False

--- 4. Locked states refuse in their own words. ---
  live         The exam is live and locked.
  completed    The exam is completed and locked.

--- 5. Illegal transitions are refused by the state object. ---
  Archive a draft          -> A draft is deleted, not archived.
  Archive a live exam      -> Wait for the exam to complete, or close it early.
  Publish twice            -> Only a draft can be published.

=== Key Insight ===
Each state owns its rules and its next state. The context only holds the
current state and forwards calls. Adding a new state means adding one class;
no existing switch or caller needs to change.
```

## How It Works

1. **The context holds exactly one state.** `ExamLifecycle` stores an `IExamState` in a private
   setter. The client can read the current state but cannot replace it directly.
2. **Every request is forwarded.** `GetEditRefusal`, `CanPublish`, `CanArchive` and `CanDelete`
   delegate to `State`. The context contains no `switch` and no status string.
3. **Each state answers with its own rules.** `DraftState` lets an author edit; `ScheduledState`
   lets only an Exams administrator edit; the three locked states return their own sentence.
4. **The state chooses the next state.** `DraftState.Publish()` returns a `ScheduledState`.
   `ScheduledState.Archive()` returns an `ArchivedState`. The client never constructs the next
   state.
5. **Illegal transitions are refused by the state.** Asking a draft to archive throws
   `TransitionRefusedException` with the draft's message. Asking a live exam to archive throws a
   different message. The caller does not pre-check the status.
6. **The shared base removes duplication.** `ExamStateBase` owns the "must be an author" gate and
   returns `false` for capabilities that a state does not override.
7. **Adding a state is additive.** A new `CancelledState` would implement the interface and return
   its own rules and transitions. No existing state, context method or caller changes.

## Why State Fits Here

An exam is a long-lived object whose rules genuinely change over time. The same `Publish` call is
legal on a draft and illegal on a scheduled exam; the same `GetEditRefusal` call returns `null`
for one state and a sentence for another. That is behaviour changing with state, not a value being
looked up.

Keeping each state in its own class gives three concrete benefits:

- **The rules for one state sit together.** A reviewer can read `LiveState` and see everything
  that is true while the window is open.
- **The compiler enforces completeness.** A new state must implement the interface, so no method
  silently falls through a forgotten `case`.
- **The caller stays simple.** Controllers and services call the context; they do not repeat a
  five-branch status check before every operation.

## State vs. Strategy

Both patterns use an interface, concrete implementations and a context that delegates. The
difference is intent:

| Question | State | Strategy |
|----------|-------|----------|
| What changes? | The object's behaviour over its lifecycle | The algorithm chosen for one task |
| Who chooses the implementation? | The states themselves, through transitions | The client or a factory |
| Is the next implementation known to the current one? | Yes — `DraftState` creates `ScheduledState` | Usually no — strategies do not transition into each other |
| Typical examples | Order, ticket, document, exam, approval lifecycle | Pricing, compression, routing, sorting, payment gateway |

Use State when the transitions are part of the domain. Use Strategy when the caller picks an
algorithm and the choice does not change the object's lifecycle.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | Each state owns the rules for exactly one lifecycle state. |
| **Open/Closed** | A new state is a new class; existing states and the context stay closed to modification. |
| **Liskov Substitution** | Every concrete state can replace another behind `IExamState` without the context changing. |
| **Interface Segregation** | `IExamState` declares only the questions and transitions the lifecycle needs. |
| **Dependency Inversion** | `ExamLifecycle` depends on the `IExamState` abstraction, not on a concrete state. |
| **Encapsulation** | The context's state setter is private; only a state transition can change it. |

## Real-World Analogy

Think of a traffic light. The same signal — "go" — means different things depending on the current
light: green allows movement, red forbids it, and yellow warns that red is next. The light does not
ask a central controller to look up a table for every car. Each phase owns its behaviour and knows
which phase follows it. The State pattern models exactly that: one object, many phases, each phase
with its own rules and its own successor.

## When to Use

- An object's behaviour depends on a status or phase and changes as that phase changes.
- The same operation is legal in some states and illegal in others.
- Transitions have real domain rules, not just a field assignment.
- Status checks are repeated across several methods or services.
- You want each state's rules to be readable and testable in isolation.

## When Not to Use

- There are only two states and one boolean check; a class hierarchy is more ceremony than value.
- The "state" is only a label for display and never changes behaviour.
- The rules are simple data lookups that belong in a table or configuration, not in objects.
- The object must be serialized frequently and the state classes carry dependencies; persist the
  state name and rebuild the object instead.
- State transitions are owned by an external workflow engine or database function; duplicating them
  in C# classes would create two sources of truth.

## Full Source Code

### `state-design-pattern.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>
  </PropertyGroup>

</Project>
```

### `Models/ExamUser.cs`

```csharp
namespace StateDesignPattern.Models;

/// <summary>
/// Who is asking. The lifecycle reads only these two capabilities, so the state classes stay
/// free of authentication, database or framework concerns.
/// </summary>
public sealed record ExamUser(bool MayAuthor, bool MayManage)
{
    public static ExamUser Author { get; } = new(MayAuthor: true, MayManage: false);

    public static ExamUser Admin { get; } = new(MayAuthor: true, MayManage: true);

    public static ExamUser Employee { get; } = new(MayAuthor: false, MayManage: false);

    public override string ToString() =>
        MayManage ? "Exams admin" : MayAuthor ? "Exam author" : "Employee";
}
```

### `Models/TransitionRefusedException.cs`

```csharp
namespace StateDesignPattern.Models;

/// <summary>Thrown when a state is asked for a transition that it does not allow.</summary>
public sealed class TransitionRefusedException(string message) : InvalidOperationException(message);
```

### `State/IExamState.cs`

```csharp
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
```

### `State/ExamStateBase.cs`

```csharp
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
```

### `State/Messages.cs`

```csharp
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
```

### `States/DraftState.cs`

```csharp
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
```

### `States/ScheduledState.cs`

```csharp
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
```

### `States/LiveState.cs`

```csharp
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
```

### `States/CompletedState.cs`

```csharp
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
```

### `States/ArchivedState.cs`

```csharp
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
```

### `Context/ExamLifecycle.cs`

```csharp
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
```

### `Program.cs`

```csharp
using StateDesignPattern.Context;
using StateDesignPattern.Models;
using StateDesignPattern.State;
using StateDesignPattern.States;

Console.WriteLine("=== State Design Pattern — Exam Lifecycle ===\n");
Console.WriteLine("CONTEXT   ExamLifecycle  holds the current state and forwards every call");
Console.WriteLine("STATE     IExamState     one object per lifecycle state");
Console.WriteLine("CONCRETE  Draft · Scheduled · Live · Completed · Archived\n");

var author = ExamUser.Author;
var admin = ExamUser.Admin;

IExamState[] states =
[
    new DraftState(),
    new ScheduledState(),
    new LiveState(),
    new CompletedState(),
    new ArchivedState(),
];

Console.WriteLine("--- 1. Who may edit? Each state answers for itself. ---");
Console.WriteLine($"  {"State",-12} {"Author",-50} Admin");

foreach (var state in states)
{
    var authorResult = state.GetEditRefusal(author) ?? "Allowed";
    var adminResult = state.GetEditRefusal(admin) ?? "Allowed";
    Console.WriteLine($"  {state.Name,-12} {authorResult,-50} {adminResult}");
}

Console.WriteLine("\n--- 2. What may be done? Capabilities also belong to the state. ---");
Console.WriteLine($"  {"State",-12} {"Publish",-10} {"Archive",-10} Delete   (as an Exams admin)");

foreach (var state in states)
{
    Console.WriteLine(
        $"  {state.Name,-12} {state.CanPublish(admin),-10} {state.CanArchive(admin),-10} {state.CanDelete(admin)}");
}

Console.WriteLine("\n--- 3. A draft moves through the lifecycle; the context never switches. ---");
var exam = new ExamLifecycle("Safety induction", new DraftState());
Show("Created", exam);
exam.Publish();
Show("Published", exam);
exam.Archive();
Show("Archived", exam);

Console.WriteLine("\n--- 4. Locked states refuse in their own words. ---");
foreach (var state in new IExamState[] { new LiveState(), new CompletedState() })
{
    var locked = new ExamLifecycle("Fire drill", state);
    Console.WriteLine($"  {state.Name,-12} {locked.GetEditRefusal(author)}");
}

Console.WriteLine("\n--- 5. Illegal transitions are refused by the state object. ---");
TryTransition("Archive a draft", () => new ExamLifecycle("Fire drill", new DraftState()).Archive());
TryTransition("Archive a live exam", () => new ExamLifecycle("Fire drill", new LiveState()).Archive());
TryTransition("Publish twice", () =>
{
    var twice = new ExamLifecycle("Fire drill", new DraftState());
    twice.Publish();
    twice.Publish();
});

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Each state owns its rules and its next state. The context only holds the");
Console.WriteLine("current state and forwards calls. Adding a new state means adding one class;");
Console.WriteLine("no existing switch or caller needs to change.");

static void Show(string moment, ExamLifecycle exam)
{
    Console.WriteLine($"  {moment,-12} {exam.Describe()}");
}

static void TryTransition(string title, Action transition)
{
    try
    {
        transition();
        Console.WriteLine($"  {title,-24} -> unexpectedly allowed");
    }
    catch (TransitionRefusedException ex)
    {
        Console.WriteLine($"  {title,-24} -> {ex.Message}");
    }
}
```

## Notes for Production Use

- **Persist the state name, rebuild the object.** Store `draft`, `scheduled`, `live`, `completed`
  or `archived` in the database, then map it to a state class when the object is loaded.
- **Keep state classes free of I/O.** The states in this example are pure. Database writes,
  logging and notifications belong in the service that calls the context.
- **Check authorization separately from state.** This example combines the author gate with the
  state rule because both answer the same question. In a larger system, resolve the user first,
  then pass only the capabilities the state needs.
- **Decide who owns transitions.** Here each state returns its successor. If transitions need
  database transactions, an audit trail or a workflow engine, let the service perform the write
  and then replace the state through a context method.
- **Do not duplicate the lifecycle.** If the database or a workflow engine already computes the
  state, use that result to build the state object rather than reimplementing the rules in C#.
