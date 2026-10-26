namespace VisitorDesignPattern;

/// <summary>A leaf element: SQL built and executed at runtime, such as <c>EXEC sp_executesql @sql</c>.</summary>
public sealed class DynamicSqlStatement : SqlNode
{
    public override void Accept(SqlVisitor visitor) => visitor.Visit(this);
}
