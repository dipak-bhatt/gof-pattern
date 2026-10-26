using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;

namespace TemplateMethodDesignPattern.Template;

/// <summary>
/// TEMPLATE METHOD — the fixed import sequence every sheet runs.
/// </summary>
/// <remarks>
/// <see cref="SaveBatchAsync"/> is the template method and is deliberately not virtual:
/// no sheet may skip the feature check, read rows out of sheet order, start its own
/// transaction, or decide whether a refusal rolls the batch back.
/// <see cref="SaveRowsAsync"/> is the single primitive operation a sheet supplies.
/// </remarks>
public abstract class BulkImportSaver<TRow>(
    IImportFeatureSwitch featureSwitch,
    IUnitOfWork unitOfWork) where TRow : class
{
    /// <summary>Which sheet this saver handles.</summary>
    public abstract ImportSheet Sheet { get; }

    /// <summary>One valid row of the sheet, as the mapper resolved it.</summary>
    protected sealed record ImportedRow(int RowNumber, TRow Row);

    /// <summary>
    /// THE TEMPLATE METHOD. The same four steps for every sheet:
    /// <list type="number">
    /// <item>refuse while bulk import is disabled;</item>
    /// <item>restore the sheet's own row order;</item>
    /// <item>run the sheet's <see cref="SaveRowsAsync"/> inside one transaction;</item>
    /// <item>report the first refusal, which rolls back every group saved before it.</item>
    /// </list>
    /// </summary>
    public async Task<ServiceResult> SaveBatchAsync(IReadOnlyList<ImportRecord<TRow>> records)
    {
        if (!await featureSwitch.IsEnabledAsync().ConfigureAwait(false))
            return ServiceResult.Fail("Bulk import is not enabled for this organization.");

        var rows = new List<ImportedRow>();
        foreach (var record in records.OrderBy(r => r.RowNumber))
            rows.Add(new ImportedRow(record.RowNumber, record.Row));

        return await unitOfWork
            .ExecuteInTransactionAsync(() => SaveRowsAsync(rows))
            .ConfigureAwait(false);
    }

    /// <summary>
    /// PRIMITIVE OPERATION — the one step that varies per sheet. It saves the rows through
    /// the module's own services, inside the transaction the template method opened.
    /// </summary>
    protected abstract Task<ServiceResult> SaveRowsAsync(IReadOnlyList<ImportedRow> rows);

    /// <summary>What a service said, as one sentence for a refusal.</summary>
    protected static string Said(ServiceResult result)
        => string.IsNullOrWhiteSpace(result.Message) ? "The row was refused." : result.Message;
}
