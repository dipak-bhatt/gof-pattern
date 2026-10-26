using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;
using TemplateMethodDesignPattern.Template;

namespace TemplateMethodDesignPattern.Savers;

/// <summary>
/// CONCRETE CLASS — saves the Products sheet: the rows of one category become one create
/// call, and categories are saved in the order the sheet first names them.
/// </summary>
public sealed class ProductImportSaver(
    IImportFeatureSwitch featureSwitch,
    IUnitOfWork unitOfWork,
    IProductService productService)
    : BulkImportSaver<ProductRow>(featureSwitch, unitOfWork)
{
    public override ImportSheet Sheet => ImportSheet.Product;

    protected override async Task<ServiceResult> SaveRowsAsync(IReadOnlyList<ImportedRow> rows)
    {
        var saved = 0;
        foreach (var group in rows.GroupBy(r => r.Row.CategoryId))
        {
            var members = group.ToList();
            var result = await productService
                .CreateAsync(new ProductBatchInput(
                    group.Key,
                    members[0].Row.CategoryCode,
                    [.. members.Select(m => m.Row.Name)]))
                .ConfigureAwait(false);

            if (!result.Status)
            {
                var rowNumbers = string.Join(", ", members.Select(m => m.RowNumber));
                return ServiceResult.Fail($"Rows {rowNumbers}: {Said(result)}");
            }

            saved += members.Count;
        }

        return ServiceResult.Ok($"{saved} product(s) imported.");
    }
}
