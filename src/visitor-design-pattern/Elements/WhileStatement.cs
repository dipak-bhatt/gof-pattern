namespace VisitorDesignPattern;

/// <summary>
/// A nonterminal element. Its body is visited inside a new scope, which lets a visitor
/// distinguish top-level statements from statements nested inside the loop.
/// </summary>
public sealed class WhileStatement(SqlNode body) : SqlNode
{
    public SqlNode Body { get; } = body;

    public override void Accept(SqlVisitor visitor)
    {
        visitor.Visit(this);
        visitor.EnterScope();
        Body.Accept(visitor);
        visitor.ExitScope();
    }
}
