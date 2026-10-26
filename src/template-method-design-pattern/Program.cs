using TemplateMethodDesignPattern.Infrastructure;
using TemplateMethodDesignPattern.Models;
using TemplateMethodDesignPattern.Savers;

Console.WriteLine("=== Template Method Design Pattern — Catalog Bulk Import ===\n");
Console.WriteLine("TEMPLATE METHOD   BulkImportSaver.SaveBatchAsync   fixed sequence (not virtual)");
Console.WriteLine("PRIMITIVE STEP    SaveRowsAsync                    the one step each sheet supplies");
Console.WriteLine("CONCRETE          CategoryImportSaver · ProductImportSaver · BundleImportSaver\n");

Console.WriteLine("Without the pattern, every sheet repeats the same surroundings:");
Console.WriteLine("  1. check the feature switch");
Console.WriteLine("  2. order the rows by spreadsheet row number");
Console.WriteLine("  3. open one transaction");
Console.WriteLine("  4. save the groups");
Console.WriteLine("  5. roll everything back on the first refusal");
Console.WriteLine("Only the grouping rule differs, so that one step moves into a subclass.\n");

var enabled = new FeatureSwitch(enabled: true);
var catalog = new InMemoryCatalog();

var categorySaver = new CategoryImportSaver(
    enabled, new InMemoryUnitOfWork(catalog), new InMemoryCategoryService(catalog));

var productSaver = new ProductImportSaver(
    enabled, new InMemoryUnitOfWork(catalog), new InMemoryProductService(catalog));

var bundleSaver = new BundleImportSaver(
    enabled, new InMemoryUnitOfWork(catalog), new InMemoryBundleService(catalog));

Console.WriteLine("--- 1. Three sheets, three subclasses, one unchanged method ---");

catalog.ClearCalls();
var categoryOutcome = await categorySaver.SaveBatchAsync(
[
    new(3, new CategoryRow("OPS", "Operations")),
    new(1, new CategoryRow("FIN", "Finance")),
    new(2, new CategoryRow("SAL", "Sales")),
]);
Console.WriteLine($"  {categorySaver.Sheet,-8} -> {categoryOutcome.Message}");
Console.WriteLine($"    sheet order  -> {string.Join(", ", catalog.Categories.Select(c => c.Code))}");
Console.WriteLine($"    one per row  -> {string.Join(" | ", catalog.Calls)}");

catalog.ClearCalls();
var productOutcome = await productSaver.SaveBatchAsync(
[
    new(4, new ProductRow(20, "SAL", "Consulting")),
    new(1, new ProductRow(10, "FIN", "Bookkeeping")),
    new(3, new ProductRow(10, "FIN", "Payroll")),
    new(2, new ProductRow(20, "SAL", "Support plan")),
]);
Console.WriteLine($"\n  {productSaver.Sheet,-8} -> {productOutcome.Message}");
Console.WriteLine("    one per group, not per row:");
PrintCalls(catalog.Calls);

catalog.ClearCalls();
var bundleOutcome = await bundleSaver.SaveBatchAsync(
[
    new(3, new BundleRow("Office starter", 30)),
    new(1, new BundleRow("Finance starter", 10)),
    new(2, new BundleRow("Finance starter", 11)),
]);
Console.WriteLine($"\n  {bundleSaver.Sheet,-8} -> {bundleOutcome.Message}");
Console.WriteLine("    one per bundle name:");
PrintCalls(catalog.Calls);

Console.WriteLine("\n--- 2. A refusal in the middle rolls back the whole batch ---");
var refusedCatalog = new InMemoryCatalog();
refusedCatalog.Refuse("category:SAL");

var refusedSaver = new CategoryImportSaver(
    enabled,
    new InMemoryUnitOfWork(refusedCatalog),
    new InMemoryCategoryService(refusedCatalog));

var refusedOutcome = await refusedSaver.SaveBatchAsync(
[
    new(1, new CategoryRow("FIN", "Finance")),
    new(2, new CategoryRow("SAL", "Sales")),
    new(3, new CategoryRow("OPS", "Operations")),
]);
Console.WriteLine($"  outcome      -> {refusedOutcome.Status}: {refusedOutcome.Message}");
Console.WriteLine($"  persisted    -> {refusedCatalog.Categories.Count} categories (Finance was written, then rolled back)");
PrintCalls(refusedCatalog.Calls);

Console.WriteLine("\n--- 3. The feature guard is fixed too, so no row is touched ---");
var disabledCatalog = new InMemoryCatalog();
var disabledSaver = new CategoryImportSaver(
    new FeatureSwitch(enabled: false),
    new InMemoryUnitOfWork(disabledCatalog),
    new InMemoryCategoryService(disabledCatalog));

var disabledOutcome = await disabledSaver.SaveBatchAsync(
[
    new(1, new CategoryRow("FIN", "Finance")),
]);
Console.WriteLine($"  outcome      -> {disabledOutcome.Message}");
Console.WriteLine($"  service calls-> {disabledCatalog.Calls.Count} (SaveRowsAsync never ran)");

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Template Method handles \"same steps, one different step\" with inheritance.");
Console.WriteLine("The base class owns the sequence: feature check, sheet order, one transaction, and");
Console.WriteLine("rollback on the first refusal. Each subclass owns only what to do with its rows:");
Console.WriteLine("categories save one per row, products group by category, bundles group by name.");
Console.WriteLine();
Console.WriteLine("Adding a fourth sheet means adding one subclass that overrides SaveRowsAsync.");
Console.WriteLine("SaveBatchAsync never changes, so a new sheet cannot forget the transaction, lose");
Console.WriteLine("the sheet order, or bypass the feature switch.");

static void PrintCalls(IReadOnlyList<string> calls)
{
    foreach (var call in calls)
        Console.WriteLine($"      -> {call}");
}
