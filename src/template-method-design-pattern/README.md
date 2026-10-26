# Template Method Design Pattern

## Definition

The Template Method pattern **defines the skeleton of an algorithm in a base class and lets
subclasses supply one or more individual steps**. The base class owns the order of the steps,
including the parts that must never change; a subclass overrides only the primitive operation
that genuinely differs.

This example models a **catalog bulk import**. Three spreadsheets arrive from a customer:

- **Categories** — one new category per row.
- **Products** — the rows of one category become one batch.
- **Bundles** — the rows that share a bundle name become one bundle.

Every sheet needs the same surroundings: check that bulk import is enabled, restore the sheet's
row order, run the save inside one transaction, and roll the whole batch back on the first
refusal. Only the grouping rule differs, so only that step is left to the subclass.

## The Problem

Without the pattern, every sheet repeats the same scaffolding:

```csharp
public async Task<ServiceResult> SaveCategoriesAsync(IReadOnlyList<Row> records)
{
    if (!await featureSwitch.IsEnabledAsync())
        return ServiceResult.Fail("Bulk import is not enabled for this organization.");

    var rows = records.OrderBy(r => r.RowNumber).ToList();

    return await unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        foreach (var row in rows)
        {
            var result = await categoryService.CreateAsync(row);
            if (!result.Status)
                return ServiceResult.Fail(result.Message);
        }

        return ServiceResult.Ok("Categories imported.");
    });
}
```

A second sheet copies the method and changes only the loop:

```csharp
public async Task<ServiceResult> SaveProductsAsync(IReadOnlyList<Row> records)
{
    if (!await featureSwitch.IsEnabledAsync())
        return ServiceResult.Fail("Bulk import is not enabled for this organization.");

    var rows = records.OrderBy(r => r.RowNumber).ToList();

    return await unitOfWork.ExecuteInTransactionAsync(async () =>
    {
        foreach (var group in rows.GroupBy(r => r.CategoryId))
        {
            var result = await productService.CreateAsync(group);
            if (!result.Status)
                return ServiceResult.Fail(result.Message);
        }

        return ServiceResult.Ok("Products imported.");
    });
}
```

This is readable for two sheets. It stops being safe when:

- a third sheet copies the same guard and transaction again;
- one sheet forgets to order by row number, so error messages name the wrong row;
- one sheet opens its own transaction and loses the all-or-nothing guarantee;
- the feature-switch rule changes and must be found in every copy;
- a new sheet is written by copying an old one, including its old mistakes.

The repeated code is not the point of the sheet. The grouping rule is.

## The Solution

Move the fixed sequence into an abstract base class and expose the varying step as a protected
abstract method:

```csharp
public abstract class BulkImportSaver<TRow>(
    IImportFeatureSwitch featureSwitch,
    IUnitOfWork unitOfWork) where TRow : class
{
    public abstract ImportSheet Sheet { get; }

    protected sealed record ImportedRow(int RowNumber, TRow Row);

    // TEMPLATE METHOD - not virtual: subclasses cannot change the sequence.
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

    // PRIMITIVE OPERATION - the one step each sheet supplies.
    protected abstract Task<ServiceResult> SaveRowsAsync(IReadOnlyList<ImportedRow> rows);

    protected static string Said(ServiceResult result)
        => string.IsNullOrWhiteSpace(result.Message) ? "The row was refused." : result.Message;
}
```

A concrete sheet is then only its own rule:

```csharp
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
                return ServiceResult.Fail(Said(result));

            saved += members.Count;
        }

        return ServiceResult.Ok($"{saved} product(s) imported.");
    }
}
```

The subclass never sees the feature switch, never sorts the rows, never opens a transaction, and
never decides what happens after a refusal. It only decides how its rows become create calls.

## Structure

```text
                         Client
                       Program.cs
                            │
                            │ SaveBatchAsync(records)
                            ▼
              ┌──────────────────────────────┐
              │ ABSTRACT CLASS               │
              │ BulkImportSaver<TRow>        │
              │                              │
              │ SaveBatchAsync()  ← template │  fixed:
              │   guard                      │   feature switch
              │   order rows                 │   sheet order
              │   transaction                │   transaction
              │     SaveRowsAsync()  ← hook  │   rollback
              │   rollback on refusal        │
              └──────────────┬───────────────┘
                             │ overrides only SaveRowsAsync
        ┌────────────────────┼────────────────────┐
        ▼                    ▼                    ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│ CategoryImport   │ │ ProductImport    │ │ BundleImport     │
│ Saver            │ │ Saver            │ │ Saver            │
│ one call per row │ │ group by category│ │ group by name    │
└──────────────────┘ └──────────────────┘ └──────────────────┘
```

The base class calls the subclass, not the other way around. That inversion is what makes the
sequence fixed: `SaveBatchAsync` stays in control and calls `SaveRowsAsync` at exactly the right
point.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Abstract class** | `BulkImportSaver<TRow>` | Owns the template method and the fixed sequence |
| **Template method** | `SaveBatchAsync` | Runs guard → order → transaction → rollback; not virtual |
| **Primitive operation** | `SaveRowsAsync` | The abstract step each sheet implements |
| **Concrete classes** | `CategoryImportSaver`, `ProductImportSaver`, `BundleImportSaver` | Supply only the grouping and save rule |
| **Data** | `ImportRecord<TRow>`, `CategoryRow`, `ProductRow`, `BundleRow` | Sheet rows and their spreadsheet row numbers |
| **Supporting services** | `IImportFeatureSwitch`, `IUnitOfWork`, `ICategoryService`, `IProductService`, `IBundleService` | The dependencies the sequence and the steps use |
| **Client** | `Program.cs` | Builds the savers and drives the example |

## Project Layout

```text
template-method-design-pattern/
├── Contracts/
│   └── IImportContracts.cs          — feature switch, transaction, module services
├── Infrastructure/
│   ├── FeatureSwitch.cs             — on/off switch for the demo
│   ├── InMemoryCatalog.cs           — fake database with snapshot and call log
│   ├── InMemoryImportServices.cs    — the module create paths
│   └── InMemoryUnitOfWork.cs        — snapshot, run, restore on failure
├── Models/
│   ├── ImportInput.cs               — what each create call accepts
│   └── ImportModel.cs               — sheet enum, rows, results
├── Savers/
│   ├── BundleImportSaver.cs         — group by bundle name
│   ├── CategoryImportSaver.cs       — one call per row
│   └── ProductImportSaver.cs        — group by category
├── Template/
│   └── BulkImportSaver.cs           — TEMPLATE METHOD and primitive operation
├── Program.cs                       — CLIENT and demonstration
├── README.md
└── template-method-design-pattern.csproj
```

## How to Run

```bash
cd src/template-method-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Template Method Design Pattern — Catalog Bulk Import ===

TEMPLATE METHOD   BulkImportSaver.SaveBatchAsync   fixed sequence (not virtual)
PRIMITIVE STEP    SaveRowsAsync                    the one step each sheet supplies
CONCRETE          CategoryImportSaver · ProductImportSaver · BundleImportSaver

Without the pattern, every sheet repeats the same surroundings:
  1. check the feature switch
  2. order the rows by spreadsheet row number
  3. open one transaction
  4. save the groups
  5. roll everything back on the first refusal
Only the grouping rule differs, so that one step moves into a subclass.

--- 1. Three sheets, three subclasses, one unchanged method ---
  Category -> 3 category(s) imported.
    sheet order  -> FIN, SAL, OPS
    one per row  -> category.CreateAsync(FIN, Finance) | category.CreateAsync(SAL, Sales) | category.CreateAsync(OPS, Operations)

  Product  -> 4 product(s) imported.
    one per group, not per row:
      -> product.CreateAsync(category FIN, 2 product(s))
      -> product.CreateAsync(category SAL, 2 product(s))

  Bundle   -> 2 bundle(s) imported.
    one per bundle name:
      -> bundle.CreateAsync(Finance starter, 2 product(s))
      -> bundle.CreateAsync(Office starter, 1 product(s))

--- 2. A refusal in the middle rolls back the whole batch ---
  outcome      -> False: Row 2: Category 'Sales' already exists.
  persisted    -> 0 categories (Finance was written, then rolled back)
      -> category.CreateAsync(FIN, Finance)
      -> category.CreateAsync(SAL, Sales)
      -> transaction rolled back - every group saved before the refusal is gone

--- 3. The feature guard is fixed too, so no row is touched ---
  outcome      -> Bulk import is not enabled for this organization.
  service calls-> 0 (SaveRowsAsync never ran)

=== Key Insight ===
Template Method handles "same steps, one different step" with inheritance.
The base class owns the sequence: feature check, sheet order, one transaction, and
rollback on the first refusal. Each subclass owns only what to do with its rows:
categories save one per row, products group by category, bundles group by name.

Adding a fourth sheet means adding one subclass that overrides SaveRowsAsync.
SaveBatchAsync never changes, so a new sheet cannot forget the transaction, lose
the sheet order, or bypass the feature switch.
```

## How It Works

1. **The base class owns the sequence.** `SaveBatchAsync` performs the feature check, restores
   sheet order, opens the transaction, calls the hook, and lets the unit of work restore the
   snapshot on failure. It is not virtual, so no concrete saver can reorder or skip those steps.
2. **The hook is the only extension point.** `SaveRowsAsync` receives rows that are already in
   sheet order and already inside a transaction. It reads nothing else from the base class.
3. **Each sheet supplies one rule.** `CategoryImportSaver` saves one category per row,
   `ProductImportSaver` groups by category, and `BundleImportSaver` groups by bundle name.
4. **The transaction is opened once, by the template method.** A concrete saver cannot start its
   own transaction, so all-or-nothing behaviour is identical for every sheet.
5. **Refusals are handled in one place.** A failed `SaveRowsAsync` returns a `ServiceResult`;
   `InMemoryUnitOfWork` sees the failure and restores the snapshot. Every group saved before the
   refusal disappears with it.
6. **Adding a sheet is additive.** A fourth sheet is one subclass overriding `SaveRowsAsync`.
   The base class, the existing savers, and the tests for the sequence do not change.

## Template Method vs. Strategy

Both patterns vary part of an algorithm. The difference is how the variation is supplied:

| Question | Template Method | Strategy |
|----------|-----------------|----------|
| How does variation happen? | Inheritance: a subclass overrides a step | Composition: the context holds an object |
| Who owns the sequence? | The base class | The context, or the caller |
| When is the variation chosen? | At compile time, by the class | At runtime, by construction or a selector |
| What changes if a step is added? | The base class and every subclass | A new strategy class, usually |
| Best when | The steps and their order are fixed, one step differs | Several whole algorithms must be interchangeable |

This demo uses Template Method because the order is genuinely fixed and non-negotiable: no sheet
may bypass the feature switch or the transaction. If the whole save operation needed to be
swapped at runtime, Strategy would be the better fit.

## Why Template Method Fits Here

- **The sequence is a rule, not an implementation detail.** The feature switch and the transaction
  must be the same for every sheet.
- **Only the grouping differs.** Categories, products, and bundles share input shape and outcome;
  the difference is how rows become create calls.
- **The fixed steps are easy to forget in a copy.** Sheet order and rollback are exactly the parts
  a new sheet would omit.
- **The number of sheets grows.** Moving the skeleton into one base class keeps the fourth and
  fifth sheet small and consistent.
- **The variation is known at compile time.** A sheet is a class, not a runtime choice.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | The base class owns the import sequence; each saver owns one grouping rule. |
| **Open/Closed** | A new sheet is a new subclass; `SaveBatchAsync` stays closed for modification. |
| **Liskov Substitution** | Every saver can be used through `BulkImportSaver<TRow>` without changing the caller. |
| **Dependency Inversion** | The sequence depends on interfaces (`IImportFeatureSwitch`, `IUnitOfWork`), not concrete services. |
| **Hollywood Principle** | "Don't call us, we'll call you": the base class calls the subclass's hook, not the reverse. |
| **DRY** | The guard, ordering, transaction, and rollback exist once instead of in every sheet. |

## Real-World Analogy

Think of a standardized exam. Every candidate gets the same instructions and the same time limit,
and every paper passes through the same marking process: collect, verify the candidate number,
mark the compulsory section, mark the optional section, total, and review. The subject teacher
supplies only the marking scheme for the questions; the examination office owns the sequence.
A teacher cannot decide to skip the candidate-number check or mark the optional section first.
The procedure is the template; the marking scheme is the primitive operation.

## When to Use

- Several classes share the same algorithm with only one or two steps differing.
- The order of the steps is a rule that must not vary between implementations.
- The shared steps include a guard, a transaction, ordering, logging, or cleanup that is easy to
  forget in a copy.
- Subclasses should be able to extend one step without changing the framework around it.
- The variation is known when the class is written.

## When Not to Use

- The whole algorithm must change at runtime; use Strategy or composition instead.
- There is only one implementation, or the shared code is a couple of one-line statements.
- The variation is data, not behaviour; a configuration table or a parameter is simpler.
- Subclasses need to change the order or skip steps; that is a sign the base class is too rigid.
- Deep inheritance would be needed to combine variations; prefer composition.

## Notes for Production Use

- **Make the template method non-virtual.** If a subclass may replace the whole sequence, the
  pattern has no guarantee left.
- **Keep hooks narrow.** One abstract method per genuinely different step. A hook that does three
  unrelated things is a subclass waiting to ignore two of them.
- **Document what the hook may assume.** Here: rows are in sheet order, the transaction is open,
  and a returned failure rolls the batch back.
- **Put cross-cutting rules in the base class.** Feature switches, ordering, transactions, and
  audit are the strongest reasons to use the pattern.
- **Do not use inheritance for dependencies.** The base class receives services through its
  constructor and passes what the hook needs; it does not build them.
- **Prefer a sealed template method and protected hooks.** That leaves testing easy while keeping
  the sequence stable.
- **A new subclass should be small.** If a concrete saver grows large, the split between the
  fixed sequence and the varying step is probably wrong.

## License

MIT
