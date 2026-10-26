namespace VisitorDesignPattern;

/// <summary>A leaf element: a call to another routine, <c>EXEC schema.procedure</c>.</summary>
public sealed class ProcedureCallStatement(string procedure) : SqlNode
{
    public string Procedure { get; } = procedure;

    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
