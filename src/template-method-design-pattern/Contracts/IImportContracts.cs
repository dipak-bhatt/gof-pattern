using TemplateMethodDesignPattern.Models;

namespace TemplateMethodDesignPattern.Contracts;

/// <summary>Whether bulk import is switched on for the current organization.</summary>
public interface IImportFeatureSwitch
{
    Task<bool> IsEnabledAsync();
}

/// <summary>
/// Runs the work as one transaction. The import template method opens the transaction; a
/// concrete saver never starts one of its own.
/// </summary>
public interface IUnitOfWork
{
    Task<ServiceResult> ExecuteInTransactionAsync(Func<Task<ServiceResult>> work);
}

/// <summary>The catalog module's own create path, one per sheet type.</summary>
public interface ICategoryService
{
    Task<ServiceResult> CreateAsync(CategoryInput input);
}

public interface IProductService
{
    Task<ServiceResult> CreateAsync(ProductBatchInput input);
}

public interface IBundleService
{
    Task<ServiceResult> CreateAsync(BundleInput input);
}
