namespace VisitorDesignPattern;

/// <summary>
/// A concrete visitor that describes the shape of a routine: which tables it reads, writes
/// and calls, and which constructs make it expensive to port or maintain.
/// </summary>
public sealed class RoutineShapeVisitor : SqlVisitor
{
    private int _depth;

    public SortedSet<string> Reads { get; } = new(StringComparer.OrdinalIgnoreCase);

    public SortedSet<string> Writes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public SortedSet<string> Calls { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Statements at the routine's own level; a loop body does not inflate the count.</summary>
    public int Statements { get; private set; }

    public int TopLevelSelects { get; private set; }

    public bool HasLoops { get; private set; }

    public bool HasCursors { get; private set; }

    public bool HasDynamicSql { get; private set; }

    public bool HasTempTables { get; private set; }

    /// <summary>True when the routine is one plain select and nothing else.</summary>
    public bool IsSingleSelect =>
        Statements == 1 &&
        TopLevelSelects == 1 &&
        !HasTempTables &&
        !HasLoops &&
        !HasDynamicSql;

    public override void EnterScope() => _depth++;

    public override void ExitScope() => _depth--;

    public override void Visit(SelectStatement statement)
    {
        if (CountStatement())
            TopLevelSelects++;

        Reads.Add(statement.Table);
    }

    public override void Visit(InsertStatement statement)
    {
        CountStatement();
        Writes.Add(statement.Table);
    }

    public override void Visit(UpdateStatement statement)
    {
        CountStatement();
        Writes.Add(statement.Table);
    }

    public override void Visit(DeleteStatement statement)
    {
        CountStatement();
        Writes.Add(statement.Table);
    }

    public override void Visit(ProcedureCallStatement statement)
    {
        CountStatement();
        Calls.Add(statement.Procedure);
    }

    public override void Visit(DynamicSqlStatement statement)
    {
        CountStatement();
        HasDynamicSql = true;
    }

    public override void Visit(WhileStatement statement)
    {
        CountStatement();
        HasLoops = true;
    }

    public override void Visit(DeclareCursorStatement statement)
    {
        CountStatement();
        HasCursors = true;
    }

    public override void Visit(CreateTempTableStatement statement)
    {
        CountStatement();
        HasTempTables = true;
    }

    /// <summary>Counts a statement only at the routine's own level; returns whether it counted.</summary>
    private bool CountStatement()
    {
        if (_depth != 0)
            return false;

        Statements++;
        return true;
    }
}
