using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;

namespace TemplateMethodDesignPattern.Infrastructure;

/// <summary>
/// Snapshots the catalog, runs the saver's work, and restores the snapshot when the work
/// reports a refusal. This is where "all or nothing" lives — once, for every sheet.
/// </summary>
public sealed class InMemoryUnitOfWork(InMemoryCatalog catalog) : IUnitOfWork
{
    public async Task<ServiceResult> ExecuteInTransactionAsync(Func<Task<ServiceResult>> work)
    {
        var snapshot = catalog.TakeSnapshot();
        var outcome = await work().ConfigureAwait(false);

        if (!outcome.Status)
            catalog.Restore(snapshot);

        return outcome;
    }
}
