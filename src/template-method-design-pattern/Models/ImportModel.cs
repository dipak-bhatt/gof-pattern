namespace TemplateMethodDesignPattern.Models;

/// <summary>The three sheets the catalog importer understands.</summary>
public enum ImportSheet
{
    Category,
    Product,
    Bundle,
}

/// <summary>One valid row of a sheet, together with the spreadsheet row it came from.</summary>
public sealed record ImportRecord<TRow>(int RowNumber, TRow Row) where TRow : class;

public sealed record CategoryRow(string Code, string Name);

public sealed record ProductRow(int CategoryId, string CategoryCode, string Name);

public sealed record BundleRow(string Name, int ProductId);

/// <summary>The outcome of one save attempt.</summary>
public sealed record ServiceResult(bool Status, string Message)
{
    public static ServiceResult Ok(string message) => new(true, message);

    public static ServiceResult Fail(string message) => new(false, message);
}
