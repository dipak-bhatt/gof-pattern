namespace VisitorDesignPattern;

/// <summary>
/// The root of the object structure: the body of a stored procedure, function or view.
/// Its children are visited in order, so a visitor sees the routine exactly as it is written.
/// </summary>
public sealed class RoutineBody(string name, params SqlNode[] statements) : SqlNode
{
    public string Name { get; } = name;

    public IReadOnlyList<SqlNode> Statements { get; } = statements;

    public override void Accept(SqlVisitor visitor)
    {
        visitor.Visit(this);

        foreach (var statement in Statements)
            statement.Accept(visitor);
    }
}
