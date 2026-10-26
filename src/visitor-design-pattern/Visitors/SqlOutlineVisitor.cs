namespace VisitorDesignPattern;

/// <summary>
/// A second concrete visitor. It walks the same object structure as
/// <see cref="RoutineShapeVisitor"/> and prints it as an indented outline. No element changed
/// to make this operation possible: only a new visitor class was added.
/// </summary>
public sealed class SqlOutlineVisitor : SqlVisitor
{
    private int _depth;

    public override void EnterScope() => _depth++;

    public override void ExitScope() => _depth--;

    public override void Visit(RoutineBody body) => WriteLine($"routine {body.Name}");

    public override void Visit(SelectStatement statement) => WriteLine($"SELECT from {statement.Table}");

    public override void Visit(InsertStatement statement) => WriteLine($"INSERT into {statement.Table}");

    public override void Visit(UpdateStatement statement) => WriteLine($"UPDATE {statement.Table}");

    public override void Visit(DeleteStatement statement) => WriteLine($"DELETE from {statement.Table}");

    public override void Visit(ProcedureCallStatement statement) => WriteLine($"EXEC {statement.Procedure}");

    public override void Visit(DynamicSqlStatement statement) => WriteLine("EXEC sp_executesql (dynamic SQL)");

    public override void Visit(WhileStatement statement) => WriteLine("WHILE");

    public override void Visit(DeclareCursorStatement statement) => WriteLine($"DECLARE CURSOR {statement.Name}");

    public override void Visit(CreateTempTableStatement statement) => WriteLine($"CREATE TABLE {statement.Name}");

    private void WriteLine(string text) => Console.WriteLine($"  {new string(' ', _depth * 2)}{text}");
}
