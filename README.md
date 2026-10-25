# GoF Design Patterns

A collection of Gang of Four (GoF) design patterns implemented in C# (.NET 10). Each pattern is a standalone, runnable console project with a working example and README.

## Patterns

| Pattern | Location | Description |
|---------|----------|-------------|
| Abstract Factory | `src/abstract-factory-design-pattern/` | Creates a family of related products for one variant, so the family can never be mixed |
| Adapter | `src/adapter-design-pattern/` | Lets two incompatible interfaces work together by wrapping one of them |
| Bridge | `src/bridge-design-pattern/` | Decouples abstraction from implementation so both can vary independently |
| Builder | `src/builder-design-pattern/` | Assembles a complex immutable object step by step, validating before it is published |
| Chain of Responsibility | `src/chain-of-responsibility-design-pattern/` | Passes a request along a chain of handlers until one answers it |
| Command | `src/command-design-pattern/` | Turns each payroll step into an object — shown with immediate execution and with a background queue + worker |
| Composite | `src/composite-design-pattern/` | Treats individual objects and compositions uniformly via a tree structure |
| Decorator | `src/decorator-design-pattern/` | Adds behaviour to an object by wrapping it in same-interface objects |
| Facade | `src/facade-design-pattern/` | Provides one simple entry point over a group of subsystem calls |
| Factory Method | `src/factory-design-pattern/` | Declares one creation method that returns a product interface and decides the concrete class per call |
| Flyweight | `src/flyweight-design-pattern/` | Shares immutable state across many objects and passes the varying state in per call |
| Interpreter | `src/interpreter-design-pattern/` | Represents each rule of a small grammar as a class and evaluates the resulting expression tree |
| Iterator | `src/iterator-design-pattern/` | Walks a collection one element at a time without exposing its storage, with lazy stop rules |
| Mediator | `src/mediator-design-pattern/` | Centralises interaction between related modules behind one coordinator |
| Memento | `src/memento-design-pattern/` | Captures an object's state in an opaque snapshot so it can be restored later |
| Observer | `src/observer-design-pattern/` | Notifies many independent subscribers when a subject changes, without the subject knowing them |
| Prototype | `src/prototype-design-pattern/` | Copies an existing object, including its whole child graph, instead of building a new one from scratch |
| Proxy | `src/proxy-design-pattern/` | Stands in for an object with the same interface — routes a call to one of several subjects, or guards access to a protected one |
| Singleton | `src/singleton-design-pattern/` | Ensures a class has exactly one instance with a global point of access |

## Structure

```
src/
├── abstract-factory-design-pattern/ — Abstract Factory pattern demo
├── adapter-design-pattern/     — Adapter pattern demo
├── bridge-design-pattern/      — Bridge pattern demo
├── builder-design-pattern/     — Builder pattern demo
├── chain-of-responsibility-design-pattern/ — Chain of Responsibility pattern demo
├── command-design-pattern/     — Command pattern demo
├── composite-design-pattern/   — Composite pattern demo
├── decorator-design-pattern/   — Decorator pattern demo
├── facade-design-pattern/      — Facade pattern demo
├── factory-design-pattern/     — Factory Method pattern demo
├── flyweight-design-pattern/   — Flyweight pattern demo
├── interpreter-design-pattern/ — Interpreter pattern demo (formula evaluator)
├── iterator-design-pattern/    — Iterator pattern demo (tenant run rotation)
├── mediator-design-pattern/    — Mediator pattern demo (approval workflow)
├── memento-design-pattern/     — Memento pattern demo (undo and redo)
├── observer-design-pattern/    — Observer pattern demo (event-driven notifications)
├── prototype-design-pattern/   — Prototype pattern demo (deep copy)
├── proxy-design-pattern/       — Proxy pattern demo (Routing + Protection)
└── singleton-design-pattern/   — Singleton pattern demo
```

## Running

```bash
cd src/abstract-factory-design-pattern
dotnet run

cd src/adapter-design-pattern
dotnet run

cd src/bridge-design-pattern
dotnet run

cd src/builder-design-pattern
dotnet run

cd src/chain-of-responsibility-design-pattern
dotnet run

cd src/command-design-pattern
dotnet run

cd src/composite-design-pattern
dotnet run

cd src/decorator-design-pattern
dotnet run

cd src/facade-design-pattern
dotnet run

cd src/factory-design-pattern
dotnet run

cd src/flyweight-design-pattern
dotnet run

cd src/interpreter-design-pattern
dotnet run

cd src/iterator-design-pattern
dotnet run

cd src/mediator-design-pattern
dotnet run

cd src/memento-design-pattern
dotnet run

cd src/observer-design-pattern
dotnet run

cd src/prototype-design-pattern
dotnet run

cd src/proxy-design-pattern
dotnet run

cd src/singleton-design-pattern
dotnet run
```

## License

MIT
