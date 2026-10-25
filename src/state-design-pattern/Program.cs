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
