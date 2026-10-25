namespace ObserverDesignPattern;

/// <summary>Formats amounts the same way in every line of demo output.</summary>
public static class Currency
{
    public static string Format(decimal amount) => $"NPR {amount:N0}";
}
