using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;
using TemplateMethodDesignPattern.Template;

namespace TemplateMethodDesignPattern.Savers;

/// <summary>
/// CONCRETE CLASS — saves the Bundles sheet: the rows that share a bundle name become one
/// create call, in sheet order.
/// </summary>
public sealed class BundleImportSaver(
    IImportFeatureSwitch featureSwitch,
    IUnitOfWork unitOfWork,
    IBundleService bundleService)
    : BulkImportSaver<BundleRow>(featureSwitch, unitOfWork)
{
    public override ImportSheet Sheet => ImportSheet.Bundle;

    protected override async Task<ServiceResult> SaveRowsAsync(IReadOnlyList<ImportedRow> rows)
    {
        var saved = 0;
        foreach (var group in rows.GroupBy(r => r.Row.Name, StringComparer.OrdinalIgnoreCase))
        {
            var members = group.ToList();
            var name = members[0].Row.Name;
            var productIds = members.Select(m => m.Row.ProductId).Distinct().ToList();

            var result = await bundleService
                .CreateAsync(new BundleInput(name, productIds))
                .ConfigureAwait(false);

            if (!result.Status)
            {
                var rowNumbers = string.Join(", ", members.Select(m => m.RowNumber));
                return ServiceResult.Fail($"Rows {rowNumbers}: {Said(result)}");
            }

            saved++;
        }

        return ServiceResult.Ok($"{saved} bundle(s) imported.");
    }
}
