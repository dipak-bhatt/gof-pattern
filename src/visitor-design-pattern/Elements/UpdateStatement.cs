namespace VisitorDesignPattern;

/// <summary>A leaf element: <c>UPDATE table ...</c>.</summary>
public sealed class UpdateStatement(string table) : SqlNode
{
    public string Table { get; } = table;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
