namespace VisitorDesignPattern;

/// <summary>
/// The visitor abstraction. It has one virtual method per element kind, so each concrete
/// visitor overrides only the methods it needs. A new element kind can be added with a default
/// no-op method here without breaking the visitors that already exist.
/// </summary>
public abstract class SqlVisitor
{
    public virtual void Visit(RoutineBody body) { }

    public virtual void Visit(SelectStatement statement) { }

    public virtual void Visit(InsertStatement statement) { }

    public virtual void Visit(UpdateStatement statement) { }

    public virtual void Visit(DeleteStatement statement) { }

    public virtual void Visit(ProcedureCallStatement statement) { }

    public virtual void Visit(DynamicSqlStatement statement) { }

    public virtual void Visit(WhileStatement statement) { }

    public virtual void Visit(DeclareCursorStatement statement) { }

    public virtual void Visit(CreateTempTableStatement statement) { }

    /// <summary>Called before a nested block's children are visited.</summary>
    public virtual void EnterScope() { }

    /// <summary>Called after a nested block's children have been visited.</summary>
    public virtual void ExitScope() { }
}
