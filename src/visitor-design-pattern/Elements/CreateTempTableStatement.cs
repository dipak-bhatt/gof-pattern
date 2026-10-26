namespace VisitorDesignPattern;

/// <summary>A leaf element: a temporary table or table variable, such as <c>CREATE TABLE #totals</c>.</summary>
public sealed class CreateTempTableStatement(string name) : SqlNode
{
    public string Name { get; } = name;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
