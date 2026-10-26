using TemplateMethodDesignPattern.Models;

namespace TemplateMethodDesignPattern.Infrastructure;

/// <summary>
/// One stand-in database, so the demo can show what was saved, how it was grouped, and
/// what a refusal rolled back.
/// </summary>
public sealed class InMemoryCatalog
{
    private readonly HashSet<string> _refused = [];
    private readonly List<string> _calls = [];

    public List<CategoryInput> Categories { get; private set; } = [];

    public List<ProductBatchInput> Products { get; private set; } = [];

    public List<BundleInput> Bundles { get; private set; } = [];

    /// <summary>Every service call made, so grouping is visible: one per row or one per group.</summary>
    public IReadOnlyList<string> Calls => _calls;

    public void Refuse(string key) => _refused.Add(key);

    public bool IsRefused(string key) => _refused.Contains(key);

    public void Record(string call) => _calls.Add(call);

    public void ClearCalls() => _calls.Clear();

    public Snapshot  TakeSnapshot() => new([.. Categories], [.. Products], [.. Bundles]);

    public void Restore(Snapshot snapshot)
    {
        Categories = snapshot.Categories;
        Products = snapshot.Products;
        Bundles = snapshot.Bundles;
        _calls.Add("transaction rolled back - every group saved before the refusal is gone");
    }

    public sealed record Snapshot(
        List<CategoryInput> Categories,
        List<ProductBatchInput> Products,
        List<BundleInput> Bundles);
}
