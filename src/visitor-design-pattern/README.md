# Visitor Design Pattern — SQL Routine Analyzer

## Definition

The Visitor pattern **adds new operations to an object structure without changing the classes of
its elements**. Each element exposes one `Accept(visitor)` method, and each operation is a separate
visitor class with one method per element kind. The element's runtime type chooses the method, and
the visitor's runtime type chooses the operation — that is double dispatch.

This example analyzes a small SQL routine tree. Two visitors run over the same nodes:

- `RoutineShapeVisitor` — collects the tables the routine reads and writes, the routines it calls,
  and the constructs that make it expensive to port or maintain.
- `SqlOutlineVisitor` — prints the routine as an indented outline.

Adding a third operation, such as "find unqualified table names" or "render the routine as
PostgreSQL", means adding one visitor class. None of the element classes change.

## Problem

A SQL routine is a tree of different statement types: selects, inserts, updates, deletes, procedure
calls, loops, cursors and temporary tables. The application needs several independent questions
answered about that tree:

1. Which tables does it read?
2. Which tables does it write?
3. Which routines does it call?
4. Does it use dynamic SQL, loops, cursors or temporary tables?
5. How does the routine look as an outline?

Without the pattern, every question becomes a `switch` over the node types:

```csharp
switch (node)
{
    case SelectStatement select:
        reads.Add(select.Table);
        break;
    case InsertStatement insert:
        writes.Add(insert.Table);
        break;
    case WhileStatement:
        hasLoops = true;
        break;
    // ... every operation repeats every case
}
```

Each new operation adds another switch, and each new element kind edits every existing switch. The
Visitor pattern moves each operation into its own class and leaves the elements with a single
`Accept` method.

## Structure

```text
Client (Program.cs)
        │
        │ builds
        ▼
┌─────────────────────┐        Accept(visitor)        ┌──────────────────────────┐
│ SqlNode             │──────────────────────────────▶│ SqlVisitor               │
│  RoutineBody        │                               │  Visit(RoutineBody)      │
│  SelectStatement    │                               │  Visit(SelectStatement)  │
│  InsertStatement    │                               │  Visit(InsertStatement)  │
│  UpdateStatement    │                               │  ...                     │
│  DeleteStatement    │                               └────────────┬─────────────┘
│  ProcedureCall      │                                            │
│  DynamicSql         │                                  ┌─────────┴──────────┐
│  WhileStatement     │                                  ▼                    ▼
│  DeclareCursor      │                        ┌──────────────────┐  ┌──────────────────┐
│  CreateTempTable    │                        │ RoutineShape     │  │ SqlOutline       │
└─────────────────────┘                        │ Visitor          │  │ Visitor          │
                                               └──────────────────┘  └──────────────────┘
```

`WhileStatement` is a nonterminal element: it visits its body inside a new scope. The scope hooks
let a visitor tell top-level statements from statements nested in a loop.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Element** | `SqlNode` | Declares `Accept(visitor)` |
| **Concrete elements** | `RoutineBody`, `SelectStatement`, `InsertStatement`, `UpdateStatement`, `DeleteStatement`, `ProcedureCallStatement`, `DynamicSqlStatement`, `WhileStatement`, `DeclareCursorStatement`, `CreateTempTableStatement` | Each dispatches to the matching visitor method |
| **Visitor** | `SqlVisitor` | Declares one virtual `Visit` method per element kind |
| **Concrete visitors** | `RoutineShapeVisitor`, `SqlOutlineVisitor` | One operation each over the whole structure |
| **Client** | `Program.cs` | Builds the tree and asks a visitor to operate on it |

## Project Layout

```text
visitor-design-pattern/
├── Elements/
│   ├── SqlNode.cs                    — ELEMENT abstraction
│   ├── RoutineBody.cs                — root element
│   ├── SelectStatement.cs            — leaf element
│   ├── InsertStatement.cs            — leaf element
│   ├── UpdateStatement.cs            — leaf element
│   ├── DeleteStatement.cs            — leaf element
│   ├── ProcedureCallStatement.cs     — leaf element
│   ├── DynamicSqlStatement.cs        — leaf element
│   ├── WhileStatement.cs             — nonterminal element
│   ├── DeclareCursorStatement.cs     — leaf element
│   └── CreateTempTableStatement.cs   — leaf element
├── Visitors/
│   ├── SqlVisitor.cs                 — VISITOR abstraction
│   ├── RoutineShapeVisitor.cs        — concrete visitor: reads, writes, calls, flags
│   └── SqlOutlineVisitor.cs          — concrete visitor: outline
├── Program.cs                        — CLIENT and demonstration
├── visitor-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/visitor-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Visitor Design Pattern — SQL Routine Analyzer ===

ELEMENT     SqlNode.Accept(visitor)       every node in a SQL routine
VISITOR     SqlVisitor                    one method per element kind
CONCRETE    RoutineShapeVisitor           reads, writes, calls and flags
            SqlOutlineVisitor             prints the same tree
CLIENT      Program.cs                    builds the tree and runs both visitors

--- 1. The object structure, rendered by an outline visitor ---
  routine recalculate_order_totals
  SELECT from orders
  UPDATE orders
  INSERT into order_totals
  EXEC write_audit_log
  EXEC sp_executesql (dynamic SQL)
  WHILE
    SELECT from order_lines
  DECLARE CURSOR order_cursor
  CREATE TABLE #order_totals

--- 2. The same nodes, a second visitor: the shape operation ---
  statements      8 at the routine's own level (1 select)
  reads           orders, order_lines
  writes          orders, order_totals
  calls           write_audit_log
  dynamic SQL     yes
  temp tables     yes
  loops           yes
  cursors         yes
  single select   no

--- 3. The same visitors work on another routine without changes ---
  order_summary          single select = yes

--- 4. Without the visitor, the operation is a type switch ---
  switch (node)
  {
      case SelectStatement s: Reads.Add(s.Table); break;
      case InsertStatement s: Writes.Add(s.Table); break;
      case WhileStatement:    HasLoops = true; break;
      // ... one case per element kind, repeated by every operation
  }
  A second operation means a second switch, and every new element kind
  means editing every switch. The visitor moves each operation into its own class.

=== Key Insight ===
Visitor adds an operation to an object structure without changing the element classes.
Each element owns only Accept; the operation and its state live in the visitor. The cost
is double dispatch and an Accept method on every element, which pays off when the
structure is stable and new operations keep arriving — exactly a SQL syntax tree.
```


## How It Works

1. **Build the object structure.** `Program.cs` builds a `RoutineBody` from concrete `SqlNode`
   instances. The tree mirrors how the routine is written.
2. **Call `Accept` once.** `routine.Accept(visitor)` calls `visitor.Visit(routine)`, then walks each
   child. Every child does the same, so one call visits the whole tree.
3. **Dispatch twice.** The first dispatch is on the element type (`node.Accept`); the second is on
   the visitor type (`visitor.Visit`). That pair is what lets one operation handle every element
   without a central `switch`.
4. **Keep operation state in the visitor.** `RoutineShapeVisitor` accumulates reads, writes, calls
   and flags. The elements stay small and know nothing about that state.
5. **Add an operation by adding a class.** `SqlOutlineVisitor` reuses every element unchanged. The
   next operation, such as a PostgreSQL rewrite check, is another class in `Visitors/`.

## Adding an Operation vs. Adding an Element

| Change | With Visitor | With a type switch |
|--------|--------------|--------------------|
| New operation | Add one visitor class | Add one `switch` to every walk, or one method to every element |
| New element kind | Add the element and one virtual method on the visitor base | Edit every `switch` in every operation |

Visitor favors a **stable object structure with many operations**. If elements change more often
than operations, a plain polymorphic method on the element (or a switch) is usually simpler.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Open/Closed** | New operations are new visitors; existing elements do not change. |
| **Single Responsibility** | Elements model SQL nodes; visitors own one analysis each. |
| **Separation of concerns** | Traversal stays in `Accept`; operation state stays in the visitor. |
| **Composition** | The routine tree is composed from small elements; visitors compose over it. |
| **Double dispatch** | The element type and the visitor type together select the operation. |

## Real-World Analogy

A compiler parses source code into an abstract syntax tree, then runs several passes over it:
type checking, optimization, code generation and pretty-printing. Each pass is a visitor, and the
syntax nodes stay unchanged. This example does the same for a small SQL routine: the shape pass and
the outline pass are two independent visitors over one tree.

## When to Use

- The object structure is stable, but new operations are added often.
- Operations need to work across many unrelated element types.
- An operation needs to accumulate state while walking the structure.
- You want to keep analysis, rendering and validation out of the element classes.
- The structure is a tree or graph, such as an AST, a document, a file system or a UI hierarchy.

## When Not to Use

- The element hierarchy changes frequently; every change then touches the visitor interface.
- There are only one or two operations; a direct method or a small `switch` is clearer.
- The operation needs access to private element internals that the public API does not expose.
- Performance is critical and the extra virtual dispatch is measurable after profiling.
