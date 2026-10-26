using TemplateMethodDesignPattern.Contracts;
using TemplateMethodDesignPattern.Models;

namespace TemplateMethodDesignPattern.Infrastructure;

/// <summary>The catalog module's create paths, kept in memory for the demo.</summary>
public sealed class InMemoryCategoryService(InMemoryCatalog catalog) : ICategoryService
{
    public Task<ServiceResult> CreateAsync(CategoryInput input)
    {
        catalog.Record($"category.CreateAsync({input.Code}, {input.Name})");

        if (catalog.IsRefused($"category:{input.Code}"))
            return Task.FromResult(ServiceResult.Fail($"Category '{input.Name}' already exists."));

        catalog.Categories.Add(input);
        return Task.FromResult(ServiceResult.Ok($"Category '{input.Name}' saved."));
    }
}

public sealed class InMemoryProductService(InMemoryCatalog catalog) : IProductService
{
    public Task<ServiceResult> CreateAsync(ProductBatchInput input)
    {
        catalog.Record($"product.CreateAsync(category {input.CategoryCode}, {input.Names.Count} product(s))");

        if (catalog.IsRefused($"product:{input.CategoryId}"))
            return Task.FromResult(ServiceResult.Fail($"Not every product of '{input.CategoryCode}' could be created."));

        catalog.Products.Add(input);
        return Task.FromResult(ServiceResult.Ok($"{input.Names.Count} product(s) of '{input.CategoryCode}' saved."));
    }
}

public sealed class InMemoryBundleService(InMemoryCatalog catalog) : IBundleService
{
    public Task<ServiceResult> CreateAsync(BundleInput input)
    {
        catalog.Record($"bundle.CreateAsync({input.Name}, {input.ProductIds.Count} product(s))");

        if (catalog.IsRefused($"bundle:{input.Name}"))
            return Task.FromResult(ServiceResult.Fail($"Bundle '{input.Name}' refers to a product that does not exist."));

        catalog.Bundles.Add(input);
        return Task.FromResult(ServiceResult.Ok($"Bundle '{input.Name}' saved."));
    }
}
