# Strategy Design Pattern

## Definition

The Strategy pattern **defines a family of algorithms, puts each one behind the same interface,
and makes them interchangeable**. The client holds one strategy and calls the same method on it;
the algorithm can change without the caller changing.

This example models a **file archive job**. Three kinds of file arrive at the job:

- a legacy encrypted upload,
- a compressed file,
- a plain file that needs no transformation.

Each kind needs different work before its bytes reach the archive, but the job does not care which
work happened. It asks for a strategy and calls `ArchiveAsync`.

## The Problem

Without the pattern, the archive method contains every algorithm:

```csharp
public async Task<ArchiveResult> ArchiveAsync(ArchivedFile file, string destination)
{
    if (file.IsEncryptedUpload)
    {
        var uploadPath = $"uploads/{file.AccountId}/{Path.GetFileName(file.SourcePath)}";
        await using var source = await store.OpenReadAsync(uploadPath);
        var decrypted = decryptor.Decrypt(source);
        await using var content = new MemoryStream(decrypted);
        var written = await store.WriteAsync(destination, content);
        return new ArchiveResult("encrypted upload", destination, written, "decrypted");
    }

    if (file.IsCompressed)
    {
        await using var source = await store.OpenReadAsync(file.SourcePath);
        await using var decompressed = await compressor.DecompressAsync(source);
        var written = await store.WriteAsync(destination, decompressed);
        return new ArchiveResult("compressed file", destination, written, "decompressed");
    }

    var copied = await store.CopyAsync(file.SourcePath, destination);
    return new ArchiveResult("plain copy", destination, copied, "copied server-side");
}
```

This is readable while there are three branches. It stops being readable when:

- one algorithm grows more steps (retries, validation, encryption key rotation),
- two jobs need the same algorithms with a different loop,
- every change to one algorithm forces a re-read of the other two,
- testing one algorithm requires the whole method and all of its dependencies.

The branch is also the only place that knows the file shapes, so the archive loop and the algorithms
are tangled together.

## The Solution

Extract one class per algorithm and make them share one interface:

```csharp
public interface IFileArchiveStrategy
{
    string Name { get; }

    Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default);
}
```

The job then contains no branch:

```csharp
public sealed class FileArchiveJob(ArchiveStrategySelector selector)
{
    public async Task<ArchiveResult> ArchiveAsync(
        ArchivedFile file,
        string destinationPath,
        CancellationToken cancellationToken = default)
    {
        var strategy = selector.For(file);
        return await strategy.ArchiveAsync(file, destinationPath, cancellationToken);
    }
}
```

The original `if / else-if / else` still exists, but it moved into one small selector whose only job
is to choose:

```csharp
public IFileArchiveStrategy For(ArchivedFile file)
{
    if (file.IsEncryptedUpload)
    {
        return encryptedUpload;
    }

    if (file.IsCompressed)
    {
        return compressedFile;
    }

    return plainCopy;
}
```

The selector is a **simple factory**, not the pattern itself. The pattern is that the three
algorithms are interchangeable behind `IFileArchiveStrategy`, and the context calls the same method
for all of them.

## Structure

```text
                       Client
                     Program.cs
                          │
                          │ ArchiveAsync(file, destination)
                          ▼
               ┌─────────────────────┐
               │ CONTEXT             │
               │ FileArchiveJob      │
               └──────────┬──────────┘
                          │ asks for a strategy
                          ▼
               ┌─────────────────────┐
               │ ArchiveStrategy     │
               │ Selector            │  ← the old if / else
               └──────────┬──────────┘
                          │ returns one
                          ▼
               ┌─────────────────────┐
               │ STRATEGY            │
               │ IFileArchiveStrategy│
               └──────────┬──────────┘
                          │ implemented by
        ┌─────────────────┼──────────────────────┐
        ▼                 ▼                      ▼
┌────────────────┐ ┌────────────────┐ ┌────────────────┐
│ EncryptedUpload│ │ CompressedFile │ │ PlainCopy      │
│ read + decrypt │ │ read + inflate │ │ server-side    │
└────────────────┘ └────────────────┘ └────────────────┘
```

The context does not know which concrete strategy it received. The strategies do not know the
context, the selector or each other.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Context** | `FileArchiveJob` | Asks for a strategy and delegates the work |
| **Strategy** | `IFileArchiveStrategy` | Declares the one method all algorithms share |
| **Concrete strategies** | `EncryptedUploadStrategy`, `CompressedFileStrategy`, `PlainCopyStrategy` | Each owns one algorithm |
| **Selector** | `ArchiveStrategySelector` | The simple factory that replaces the old branch |
| **Data** | `ArchivedFile`, `ArchiveResult` | The input flags and the outcome |
| **Supporting services** | `IFileStore`, `IEncryptorDecryptor`, `IFileCompressor` | What the algorithms need, behind interfaces |
| **Client** | `Program.cs` | Builds the strategies and drives the example |

## Project Layout

```text
strategy-design-pattern/
├── Context/
│   ├── ArchiveStrategySelector.cs        — the old if / else, isolated
│   └── FileArchiveJob.cs                 — CONTEXT: delegates to one strategy
├── Contracts/
│   ├── IEncryptorDecryptor.cs            — decrypt support
│   ├── IFileArchiveStrategy.cs           — STRATEGY contract
│   ├── IFileCompressor.cs                — decompress support
│   └── IFileStore.cs                     — storage support
├── Infrastructure/
│   ├── GZipCompressor.cs                 — GZip implementation
│   ├── InMemoryFileStore.cs              — fake object store with a call log
│   └── PrefixDecryptor.cs                — fake decryptor (ENC: prefix)
├── Models/
│   ├── ArchivedFile.cs                   — input flags
│   └── ArchiveResult.cs                  — outcome
├── Strategies/
│   ├── CompressedFileStrategy.cs         — read + decompress + write
│   ├── EncryptedUploadStrategy.cs        — read uploads/ + decrypt + write
│   └── PlainCopyStrategy.cs              — server-side copy
├── Program.cs                            — CLIENT and demonstration
├── strategy-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/strategy-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Strategy Design Pattern — File Archive Job ===

CONTEXT   FileArchiveJob          asks the selector, then delegates
STRATEGY  IFileArchiveStrategy    "copy this file into the archive"
CONCRETE  EncryptedUpload · CompressedFile · PlainCopy

--- 1. Without the pattern, one method owns all three algorithms ---
  if (file.IsEncryptedUpload)
      read uploads/{accountId}/{name}; decrypt; write the destination
  else if (file.IsCompressed)
      read the stored path; decompress; write the destination
  else
      fileStore.CopyAsync(file.SourcePath, destination);

--- 2. The file flags choose one strategy per file ---
  file                       encrypted   compressed  strategy
  legacy/handbook.txt        True        False       encrypted upload
  files/roster.csv.gz        False       True        compressed file
  files/offer-letter.txt     False       False       plain copy

--- 3. The context calls the same method every time ---
  legacy/handbook.txt
    strategy -> encrypted upload (decrypted)
    store    -> OpenReadAsync("uploads/11111111-1111-1111-1111-111111111111/handbook.txt")
    store    -> WriteAsync("archive/handbook.txt", 21 bytes)
    result   -> "Employee handbook, v1"
  files/roster.csv.gz
    strategy -> compressed file (decompressed)
    store    -> OpenReadAsync("files/roster.csv.gz")
    store    -> WriteAsync("archive/roster.csv", 22 bytes)
    result   -> "id,name
1,Ram Bahadur"
  files/offer-letter.txt
    strategy -> plain copy (copied server-side)
    store    -> CopyAsync("files/offer-letter.txt" -> "archive/offer-letter.txt")
    result   -> "Offer letter for Sita Sharma"

--- 4. Three classes, one interface, one place that chooses ---
  public interface IFileArchiveStrategy
  {
      string Name { get; }
      Task<ArchiveResult> ArchiveAsync(
          ArchivedFile file, string destination, CancellationToken ct);
  }

  EncryptedUploadStrategy   reads uploads/, decrypts, writes
  CompressedFileStrategy    reads the stored path, decompresses, writes
  PlainCopyStrategy         lets the file store copy the object server-side

  ArchiveStrategySelector.For(file) is the old if / else. It is a simple
  factory, not the pattern itself: the pattern is that the three algorithms
  are interchangeable behind the interface, and the job loop does not change.

=== Key Insight ===
Strategy turns "which algorithm?" into an object. The context receives one
IFileArchiveStrategy and calls the same method on it, so the three algorithms
can be read, tested and changed independently. Adding a fourth way to obtain
the bytes (for example, downloading from a URL) means adding one class and one
line in the selector, not another branch in the middle of the archive loop.
```

## How It Works

1. **One contract.** Every algorithm implements `IFileArchiveStrategy`: a name and one
   `ArchiveAsync` method. The caller sees exactly one operation.
2. **One class per algorithm.** `EncryptedUploadStrategy` reads from `uploads/`, decrypts and
   writes. `CompressedFileStrategy` reads the stored path, decompresses and writes.
   `PlainCopyStrategy` lets the store copy the object without moving the bytes through the
   application. Each class can be read and tested on its own.
3. **The context delegates.** `FileArchiveJob` asks the selector for a strategy and calls it. It
   contains no flag test and no reading, decrypting, decompressing or copying code.
4. **Selection is separate.** `ArchiveStrategySelector` holds the old `if / else` and nothing else.
   It is the one place to change when the rule for choosing an algorithm changes.
5. **Supporting services are interfaces.** `IFileStore`, `IEncryptorDecryptor` and
   `IFileCompressor` keep the strategies free of concrete I/O, so the demo can run in memory and
   each strategy is replaceable in tests.
6. **Adding an algorithm is additive.** A fourth strategy (for example, fetching a file from a
   URL) is a new class implementing `IFileArchiveStrategy`, plus one selection rule. The job, the
   result type and the three existing strategies do not change.

## Why Strategy Fits Here

Copying a file into an archive is one task with several genuinely different algorithms. The input
and the outcome are the same; only the steps in the middle differ. That is the exact shape the
pattern is for:

- **The algorithm varies, the operation does not.** All three produce an `ArchiveResult` for the
  same destination; none of them changes the caller's loop.
- **The choice is data-driven.** The file's storage shape, not the caller's identity, decides the
  algorithm. The selector centralises that mapping.
- **The algorithms change independently.** Decryption, decompression and plain copying have
  different dependencies and different failure modes. Keeping them apart means a change to one
  does not require re-reading the other two.
- **Tests get smaller.** Each strategy can be tested with a fake store and only the dependency it
  actually uses.

## Strategy vs. State

Both patterns put behaviour behind an interface and let a context delegate. The difference is
intent and who chooses:

| Question | Strategy | State |
|----------|----------|-------|
| What changes? | The algorithm used for one task | The object's behaviour over a lifecycle |
| Who chooses the implementation? | The client, or a selector/factory | The state objects, through transitions |
| Does one implementation create the next? | Usually no | Yes — one state returns its successor |
| Typical examples | Copying, compression, sorting, pricing, routing | Orders, tickets, documents, approvals |

Use Strategy when several algorithms can do the same job and the caller picks one. Use State when
the object's legal behaviour changes as it moves through phases and the next phase is part of the
domain.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | Each strategy owns one algorithm; the selector owns only the choice. |
| **Open/Closed** | A new algorithm is a new class; the job and the existing strategies stay closed. |
| **Liskov Substitution** | Any strategy can replace another behind `IFileArchiveStrategy`. |
| **Interface Segregation** | `IFileArchiveStrategy` exposes only `Name` and `ArchiveAsync`. |
| **Dependency Inversion** | The context depends on the strategy interface, not on a concrete class. |
| **Composition over inheritance** | The job composes a strategy instead of subclassing one archive job per file shape. |

## Real-World Analogy

Think of a parcel delivery service. The order is the same — move a parcel from A to B — but the
route plan differs: same-day courier, standard courier or freight. The customer chooses a service
level; the dispatch desk hands the parcel to the chosen carrier and does not change its own process.
The carrier owns the route, the customer owns the choice, and the dispatch desk owns neither.

## When to Use

- Several algorithms do the same job and can be swapped at runtime.
- The algorithms are selected by data (a file shape, a payment method, a region) rather than by
  the caller's type.
- A long `if / else` or `switch` chooses between implementations and each branch is growing.
- You want each algorithm tested and reviewed in isolation.
- The same family of algorithms is needed by more than one job.

## When Not to Use

- There is one algorithm, or the branches are one-line expressions. A method is simpler.
- The alternatives are pure data lookups; a table or configuration is the better home.
- The algorithms cannot be described by one stable interface without adding unrelated methods.
- The caller must inspect the concrete strategy to use it; that defeats the abstraction.
- Selection needs a full workflow engine or complex state; look at State or a workflow tool first.


## Notes for Production Use

- **Keep strategies free of I/O construction.** Receive `IFileStore`, `IEncryptorDecryptor` and
  `IFileCompressor` through the constructor. The strategies describe an algorithm; the container
  supplies the clients.
- **Return one outcome type.** `ArchiveResult` lets every strategy answer in the same shape, so the
  caller never needs to know which algorithm ran.
- **Do not let the selector grow into the algorithm.** It should only choose. If a branch needs
  retries or validation, that belongs in the strategy.
- **Prefer constructor injection for a fixed strategy.** When the client already knows which
  algorithm it wants, pass `IFileArchiveStrategy` directly. Use a selector only when the choice
  depends on data.
- **Name strategies by what they do, not by how they were called.** `CompressedFileStrategy`
  survives a refactor; `ElseIfBranchStrategy` does not.
- **A new algorithm is additive.** Add the class, register it, add the selection rule. If an
  existing strategy must change to make room, the interface is probably not stable yet.
