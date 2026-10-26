namespace VisitorDesignPattern;

/// <summary>A leaf element: <c>INSERT INTO table ...</c>.</summary>
public sealed class InsertStatement(string table) : SqlNode
{
    public string Table { get; } = table;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
