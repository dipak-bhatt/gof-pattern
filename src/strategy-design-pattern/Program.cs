using System.Text;
using StrategyDesignPattern.Context;
using StrategyDesignPattern.Infrastructure;
using StrategyDesignPattern.Models;
using StrategyDesignPattern.Strategies;

Console.WriteLine("=== Strategy Design Pattern — File Archive Job ===\n");
Console.WriteLine("CONTEXT   FileArchiveJob          asks the selector, then delegates");
Console.WriteLine("STRATEGY  IFileArchiveStrategy    \"copy this file into the archive\"");
Console.WriteLine("CONCRETE  EncryptedUpload · CompressedFile · PlainCopy\n");

Console.WriteLine("--- 1. Without the pattern, one method owns all three algorithms ---");
Console.WriteLine("  if (file.IsEncryptedUpload)");
Console.WriteLine("      read uploads/{accountId}/{name}; decrypt; write the destination");
Console.WriteLine("  else if (file.IsCompressed)");
Console.WriteLine("      read the stored path; decompress; write the destination");
Console.WriteLine("  else");
Console.WriteLine("      fileStore.CopyAsync(file.SourcePath, destination);");

var accountId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var store = new InMemoryFileStore();
var decryptor = new PrefixDecryptor();
var compressor = new GZipCompressor();

store.Seed(
    $"uploads/{accountId}/handbook.txt",
    PrefixDecryptor.Encrypt("Employee handbook, v1"));

store.Seed(
    "files/roster.csv.gz",
    GZipCompressor.Compress(Encoding.UTF8.GetBytes("id,name\r\n1,Ram Bahadur")));

store.Seed(
    "files/offer-letter.txt",
    Encoding.UTF8.GetBytes("Offer letter for Sita Sharma"));

ArchivedFile[] files =
[
    new("legacy/handbook.txt", accountId, IsEncryptedUpload: true),
    new("files/roster.csv.gz", accountId, IsCompressed: true),
    new("files/offer-letter.txt", accountId),
];

var selector = new ArchiveStrategySelector(
    new EncryptedUploadStrategy(store, decryptor),
    new CompressedFileStrategy(store, compressor),
    new PlainCopyStrategy(store));

var job = new FileArchiveJob(selector);

Console.WriteLine("\n--- 2. The file flags choose one strategy per file ---");
Console.WriteLine($"  {"file",-26} {"encrypted",-11} {"compressed",-11} strategy");
foreach (var file in files)
{
    Console.WriteLine(
        $"  {file.SourcePath,-26} {file.IsEncryptedUpload,-11} {file.IsCompressed,-11} {selector.For(file).Name}");
}

Console.WriteLine("\n--- 3. The context calls the same method every time ---");
foreach (var file in files)
{
    store.Actions.Clear();
    var destination = DestinationFor(file);

    Console.WriteLine($"  {file.SourcePath}");

    var result = await job.ArchiveAsync(file, destination);
    Console.WriteLine($"    strategy -> {result.StrategyName} ({result.Detail})");

    foreach (var action in store.Actions)
    {
        Console.WriteLine($"    store    -> {action}");
    }

    Console.WriteLine($"    result   -> \"{Encoding.UTF8.GetString(store.Read(destination))}\"");
}

Console.WriteLine("\n--- 4. Three classes, one interface, one place that chooses ---");
Console.WriteLine("  public interface IFileArchiveStrategy");
Console.WriteLine("  {");
Console.WriteLine("      string Name { get; }");
Console.WriteLine("      Task<ArchiveResult> ArchiveAsync(");
Console.WriteLine("          ArchivedFile file, string destination, CancellationToken ct);");
Console.WriteLine("  }");
Console.WriteLine();
Console.WriteLine("  EncryptedUploadStrategy   reads uploads/, decrypts, writes");
Console.WriteLine("  CompressedFileStrategy    reads the stored path, decompresses, writes");
Console.WriteLine("  PlainCopyStrategy         lets the file store copy the object server-side");
Console.WriteLine();
Console.WriteLine("  ArchiveStrategySelector.For(file) is the old if / else. It is a simple");
Console.WriteLine("  factory, not the pattern itself: the pattern is that the three algorithms");
Console.WriteLine("  are interchangeable behind the interface, and the job loop does not change.");

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Strategy turns \"which algorithm?\" into an object. The context receives one");
Console.WriteLine("IFileArchiveStrategy and calls the same method on it, so the three algorithms");
Console.WriteLine("can be read, tested and changed independently. Adding a fourth way to obtain");
Console.WriteLine("the bytes (for example, downloading from a URL) means adding one class and one");
Console.WriteLine("line in the selector, not another branch in the middle of the archive loop.");

static string DestinationFor(ArchivedFile file) =>
    "archive/" + Path.GetFileName(file.SourcePath).Replace(".gz", string.Empty, StringComparison.Ordinal);
