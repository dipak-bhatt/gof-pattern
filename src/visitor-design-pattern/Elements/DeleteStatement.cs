namespace VisitorDesignPattern;

/// <summary>A leaf element: <c>DELETE FROM table ...</c>.</summary>
public sealed class DeleteStatement(string table) : SqlNode
{
    public string Table { get; } = table;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
