namespace TemplateMethodDesignPattern.Models;

public sealed record CategoryInput(string Code, string Name);

public sealed record ProductBatchInput(
    int CategoryId,
    string CategoryCode,
    IReadOnlyList<string> Names);

public sealed record BundleInput(string Name, IReadOnlyList<int> ProductIds);
