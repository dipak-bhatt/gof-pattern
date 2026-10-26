namespace VisitorDesignPattern;

/// <summary>A leaf element: <c>DECLARE cursor CURSOR ...</c>.</summary>
public sealed class DeclareCursorStatement(string name) : SqlNode
{
    public string Name { get; } = name;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
