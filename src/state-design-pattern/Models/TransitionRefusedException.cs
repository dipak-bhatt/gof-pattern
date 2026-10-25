namespace StateDesignPattern.Models;

/// <summary>Thrown when a state is asked for a transition that it does not allow.</summary>
public sealed class TransitionRefusedException(string message) : InvalidOperationException(message);
