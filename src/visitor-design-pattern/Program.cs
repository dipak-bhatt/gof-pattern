using VisitorDesignPattern;

Console.WriteLine("=== Visitor Design Pattern — SQL Routine Analyzer ===\n");
Console.WriteLine("ELEMENT     SqlNode.Accept(visitor)       every node in a SQL routine");
Console.WriteLine("VISITOR     SqlVisitor                    one method per element kind");
Console.WriteLine("CONCRETE    RoutineShapeVisitor           reads, writes, calls and flags");
Console.WriteLine("            SqlOutlineVisitor             prints the same tree");
Console.WriteLine("CLIENT      Program.cs                    builds the tree and runs both visitors\n");

var routine = new RoutineBody(
    "recalculate_order_totals",
    new SelectStatement("orders"),
    new UpdateStatement("orders"),
    new InsertStatement("order_totals"),
    new ProcedureCallStatement("write_audit_log"),
    new DynamicSqlStatement(),
    new WhileStatement(new SelectStatement("order_lines")),
    new DeclareCursorStatement("order_cursor"),
    new CreateTempTableStatement("#order_totals"));

Console.WriteLine("--- 1. The object structure, rendered by an outline visitor ---");
routine.Accept(new SqlOutlineVisitor());

Console.WriteLine("\n--- 2. The same nodes, a second visitor: the shape operation ---");
var shape = new RoutineShapeVisitor();
routine.Accept(shape);

Console.WriteLine($"  statements      {shape.Statements} at the routine's own level ({shape.TopLevelSelects} select)");
Console.WriteLine($"  reads           {Join(shape.Reads)}");
Console.WriteLine($"  writes          {Join(shape.Writes)}");
Console.WriteLine($"  calls           {Join(shape.Calls)}");
Console.WriteLine($"  dynamic SQL     {Yes(shape.HasDynamicSql)}");
Console.WriteLine($"  temp tables     {Yes(shape.HasTempTables)}");
Console.WriteLine($"  loops           {Yes(shape.HasLoops)}");
Console.WriteLine("  cursors         " + Yes(shape.HasCursors));
Console.WriteLine($"  single select   {Yes(shape.IsSingleSelect)}");

Console.WriteLine("\n--- 3. The same visitors work on another routine without changes ---");
var view = new RoutineBody("order_summary", new SelectStatement("orders"));
var viewShape = new RoutineShapeVisitor();
view.Accept(viewShape);
Console.WriteLine($"  {view.Name,-22} single select = {Yes(viewShape.IsSingleSelect)}");

Console.WriteLine("\n--- 4. Without the visitor, the operation is a type switch ---");
Console.WriteLine("  switch (node)");
Console.WriteLine("  {");
Console.WriteLine("      case SelectStatement s: Reads.Add(s.Table); break;");
Console.WriteLine("      case InsertStatement s: Writes.Add(s.Table); break;");
Console.WriteLine("      case WhileStatement:    HasLoops = true; break;");
Console.WriteLine("      // ... one case per element kind, repeated by every operation");
Console.WriteLine("  }");
Console.WriteLine("  A second operation means a second switch, and every new element kind");
Console.WriteLine("  means editing every switch. The visitor moves each operation into its own class.");

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("Visitor adds an operation to an object structure without changing the element classes.");
Console.WriteLine("Each element owns only Accept; the operation and its state live in the visitor. The cost");
Console.WriteLine("is double dispatch and an Accept method on every element, which pays off when the");
Console.WriteLine("structure is stable and new operations keep arriving — exactly a SQL syntax tree.");

static string Join(IEnumerable<string> values)
{
    var items = values.ToList();
    return items.Count == 0 ? "(none)" : string.Join(", ", items);
}

static string Yes(bool value) => value ? "yes" : "no";
