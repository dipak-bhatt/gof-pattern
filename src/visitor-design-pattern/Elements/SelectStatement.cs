namespace VisitorDesignPattern;

/// <summary>A leaf element: <c>SELECT ... FROM table</c>.</summary>
public sealed class SelectStatement(string table) : SqlNode
{
    public string Table { get; } = table;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
