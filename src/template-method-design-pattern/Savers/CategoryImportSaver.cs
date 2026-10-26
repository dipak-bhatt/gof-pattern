using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;
using TemplateMethodDesignPattern.Template;

namespace TemplateMethodDesignPattern.Savers;

/// <summary>
/// CONCRETE CLASS — saves the Categories sheet: one create call per row, in sheet order.
/// </summary>
public sealed class CategoryImportSaver(
    IImportFeatureSwitch featureSwitch,
    IUnitOfWork unitOfWork,
    ICategoryService categoryService)
    : BulkImportSaver<CategoryRow>(featureSwitch, unitOfWork)
{
    public override ImportSheet Sheet => ImportSheet.Category;

    protected override async Task<ServiceResult> SaveRowsAsync(IReadOnlyList<ImportedRow> rows)
    {
        var saved = 0;
        foreach (var imported in rows)
        {
            var result = await categoryService
                .CreateAsync(new CategoryInput(imported.Row.Code, imported.Row.Name))
                .ConfigureAwait(false);

            if (!result.Status)
                return ServiceResult.Fail($"Row {imported.RowNumber}: {Said(result)}");

            saved++;
        }

        return ServiceResult.Ok($"{saved} category(s) imported.");
    }
}
