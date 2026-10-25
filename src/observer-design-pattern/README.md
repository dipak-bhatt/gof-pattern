# Observer Design Pattern

## Definition

The Observer pattern **defines a one-to-many dependency between objects**: when one object (the
subject) changes state, every attached object (the observer) is notified automatically. The subject
knows only that its observers implement a common contract. It does not know who they are, how many
there are, or what they do.

This example models **order status notifications**:

- `OrderService` is the publisher. Placing or paying an order changes its state.
- `EventBus` is the subject. It keeps the list of observers for each topic.
- `confirmation-email`, `inventory`, `audit`, `analytics` and `loyalty-points` are observers. Each
  one decides which events matter to it.

The order workflow never calls an e-mail service, an inventory service or an audit log. It publishes
one event. Every observer that subscribed receives it.

Without the pattern, the workflow would collect every reaction:

```csharp
public async Task PayAsync(Order order)
{
    order.Status = OrderStatus.Paid;
    await _repository.SaveAsync(order);

    await _email.SendReceiptAsync(order);       // add a receipt
    await _inventory.ReserveAsync(order);       // add stock reservation
    _audit.Record(order);                       // add an audit entry
    // ...and another line for every new requirement
}
```

With the pattern, the workflow publishes and moves on:

```csharp
await bus.PublishAsync(
    OrderTopics.StatusChanged,
    new OrderStatusChanged(order.Id, order.Customer, OrderStatus.Paid, order.Total));
```

## Structure

```text
┌──────────────────────────┐
│  CLIENT                  │
│  Program.cs              │
└────────────┬─────────────┘
             │ PlaceAsync / PayAsync
             ▼
┌──────────────────────────┐        publishes         ┌──────────────────────────────┐
│  PUBLISHER               │─────────────────────────>│  SUBJECT                     │
│  OrderService            │  topic + event           │  EventBus                    │
│  knows no observer       │                          │  Subscribe / PublishAsync    │
└──────────────────────────┘                          └──────────────┬───────────────┘
                                                                     │ notifies every
                                                                     │ observer on the topic
             ┌───────────────────────────────────────────────────────┼───────────────────┐
             │                       │                    │          │                   │
             ▼                       ▼                    ▼          ▼                   ▼
     ┌───────────────┐       ┌───────────────┐    ┌───────────┐ ┌──────────┐   ┌────────────────┐
     │ confirmation- │       │ inventory     │    │ audit     │ │ analytics│   │ loyalty-points │
     │ email         │       │               │    │           │ │          │   │                │
     └───────────────┘       └───────────────┘    └───────────┘ └──────────┘   └────────────────┘
         OBSERVER               OBSERVER            OBSERVER     OBSERVER          OBSERVER
```

The arrow points one way. The bus does not know what an observer does, and an observer does not know
that the others exist. `OrderService` knows only `IEventBus`.

## Pattern Roles

| Role | Type | Responsibility |
|------|------|----------------|
| **Subject** | `IEventBus`, `EventBus` | Keeps the subscriptions and notifies every observer attached to a topic |
| **Concrete subject** | `OrderService` | Owns the order state and publishes the event when it changes |
| **Observer** | `IEventObserver<TEvent>` | Declares the one method every subscriber implements |
| **Concrete observer** | `ConfirmationEmailObserver`, `InventoryObserver`, `AuditObserver`, `AnalyticsObserver`, `LoyaltyPointsObserver` | Decide what an event means for them and apply it |
| **Event** | `OrderStatusChanged` | The immutable data handed to every observer |
| **Client** | `Program.cs` | Creates the bus, attaches observers and drives the workflow |

## Project Layout

```text
observer-design-pattern/
├── Common/
│   └── Currency.cs                     — shared amount formatting
├── Models/
│   ├── OrderStatus.cs                  — Placed, Paid, Shipped, Cancelled
│   └── OrderStatusChanged.cs           — the immutable event
├── Subject/
│   ├── IEventObserver.cs               — OBSERVER contract
│   ├── IEventBus.cs                    — SUBJECT contract (subscribe / publish)
│   └── EventBus.cs                     — CONCRETE SUBJECT
├── Observers/
│   ├── ConfirmationEmailObserver.cs    — receipt on Paid
│   ├── InventoryObserver.cs            — stock reservation on Paid
│   ├── AuditObserver.cs                — records every status
│   ├── AnalyticsObserver.cs            — added while the program runs
│   └── LoyaltyPointsObserver.cs        — illustrates a failing observer
├── Services/
│   ├── OrderTopics.cs                  — topic names
│   └── OrderService.cs                 — PUBLISHER (concrete subject)
├── Program.cs                          — CLIENT and demonstration
├── observer-design-pattern.csproj
└── README.md
```

## How to Run

```bash
cd src/observer-design-pattern
dotnet run
```

Requires the .NET 10 SDK.

## Output

```text
=== Observer Design Pattern — Order Status Notifications ===

SUBJECT    EventBus
OBSERVERS  confirmation-email · inventory · audit · analytics · loyalty-points
RULE       the publisher knows only the topic and the event
           it never holds a reference to an observer

--- 1. Three observers attach themselves to the same topic ---
  confirmation-email <- orders.status.changed
  inventory          <- orders.status.changed
  audit              <- orders.status.changed

--- 2. Place an order (Placed) ---
  orders -> orders.status.changed
  audit: ORD-1001 -> Placed (total NPR 4,200)
  note: e-mail and inventory act only when the order is Paid

--- 3. Pay the same order (Paid) ---
  orders -> orders.status.changed
  audit: ORD-1001 -> Paid (total NPR 4,200)
  confirmation-email: receipt for ORD-1001 sent to Asha Rai
  inventory: stock reserved for order ORD-1001

--- 4. Detach the audit observer; the publisher is unchanged ---
  audit: unsubscribed
  orders -> orders.status.changed
  confirmation-email: receipt for ORD-1002 sent to Bikash Shrestha
  inventory: stock reserved for order ORD-1002
  note: no audit line appears, but the other observers still run

--- 5. Attach a new observer while the program is running ---
  analytics          <- orders.status.changed
  orders -> orders.status.changed
  analytics: counted one Placed event

--- 6. One observer fails; every other observer still receives the event ---
  loyalty-points     <- orders.status.changed
  orders -> orders.status.changed
  confirmation-email: receipt for ORD-1004 sent to Hari Thapa
  inventory: stock reserved for order ORD-1004
  analytics: counted one Paid event
  bus: observer 'loyalty-points' failed: the loyalty service is unavailable
       the other observers continue; this one is simply skipped

=== Key Insight ===
The publisher owns the event and the topic, nothing else. Each observer
attaches on its own and reacts on its own: detaching one removes only that
one, and adding another is a single Subscribe call. The order workflow and
every existing observer stay untouched.
```

## How It Works

1. **The observer contract is one method.** `IEventObserver<TEvent>` has a `Name` and
   `OnEventAsync`. Nothing else is required, so a new observer is a small class.
2. **The subject keeps a list per topic.** `EventBus.Subscribe` adds a registration and returns an
   `IDisposable`. `PublishAsync` looks up the topic, copies the list and calls each registration.
3. **Detaching is disposal.** Disposing the handle removes exactly one registration. The publisher
   is not involved, so no workflow code changes when an observer leaves.
4. **Notifications fan out.** Several observers can subscribe to the same topic. The same immutable
   `OrderStatusChanged` instance reaches each of them, so one observer cannot change what the next
   one sees.
5. **Observers filter for themselves.** `ConfirmationEmailObserver` and `InventoryObserver` return
   early unless the status is `Paid`. `AuditObserver` and `AnalyticsObserver` record every status.
   The subject does not contain these rules.
6. **A failure stays local.** The bus catches an observer's exception, reports it, and continues.
   `LoyaltyPointsObserver` demonstrates this: the other observers still process the event.
7. **The publisher depends on the abstraction.** `OrderService` holds an `IEventBus`, not a list of
   concrete observers. Tests can pass a fake bus; production can pass a real broker-backed one.

### A note on threading

The demo is single-threaded and awaits observers one at a time, which keeps the output easy to
follow. A real bus may deliver observers in parallel or hand the event to independent consumers.
The important contract stays the same: the publisher does not know the observers, and each
subscriber can fail without breaking the publisher.

## Why Observer Fits This Workflow

Order status is a fact that several parts of the system care about for different reasons:

- **The publisher should not grow with every requirement.** A receipt, stock reservation, audit
  entry and analytics count are four separate concerns. Putting them in `PayAsync` makes that method
  a store of unrelated calls.
- **Observers change independently.** The e-mail template, inventory rules and analytics provider
  have different owners and release cycles. Each can change without editing the order workflow.
- **Attach and detach at runtime.** The demo detaches the audit observer and adds analytics while
  the program is running. No compile-time list and no publisher change are involved.
- **Failures should not cascade.** A loyalty service that is down must not stop receipts or stock
  reservation. Isolating each observer keeps one dependency's outage from becoming a checkout
  outage.
- **The same event serves different reactions.** One `Paid` event means "send a receipt" to one
  observer and "reserve stock" to another. The event does not need a separate version for each.

## Software Design Principles

| Principle | How this example applies it |
|-----------|-----------------------------|
| **Single Responsibility** | `OrderService` changes order state; each observer owns one reaction; `EventBus` owns subscription and delivery. |
| **Open/Closed** | A new reaction is a new observer plus one `Subscribe` call. The publisher and existing observers stay closed to modification. |
| **Liskov Substitution** | Any `IEventObserver<TEvent>` can replace another without the bus changing. |
| **Dependency Inversion** | `OrderService` depends on `IEventBus`; observers depend on `IEventObserver`; neither depends on the other's concrete class. |
| **Interface Segregation** | The observer contract has one delivery method, not a general-purpose handler with uses it does not need. |
| **Composition over inheritance** | The bus composes a list of independent observers instead of forcing behaviour into a base order class. |

## Real-World Analogy

Think of a newspaper subscription. The newspaper (the subject) prints an edition and sends it to
every subscriber on its list. It does not write a different edition for each reader, and it does not
know what each reader does with the paper. A reader can subscribe or cancel without the newspaper
changing how it prints. A reader who never reads the business section simply ignores that page.

## When to Use

- One change must trigger several independent reactions.
- The set of reactions is not known at compile time, or must change while the application runs.
- The publisher should not depend on the concrete classes that react to it.
- Observers have their own failure modes that must not stop the publisher or each other.
- You want to add or remove a reaction without editing the object that owns the state.

### When Not to Use

- There is only one reaction and no sign of another. A direct method call is clearer.
- The order of reactions matters and must be guaranteed. The pattern does not define ordering across
  independent observers; make the sequence explicit instead.
- Delivery must be durable across process crashes or restarts. An in-process bus is not enough; use
  an outbox or a message broker and make redelivery part of the design.
- The event is large or changes often. Every observer then pays the cost of a shape it may not use;
  a smaller event or a direct query can be better.

## Observer vs. Pub/Sub

This example uses the Observer pattern locally: the subject and its observers live in one process
and the bus is an in-memory implementation. The same roles map to a message broker, but the broker
adds durability, redelivery and independent consumer state. If an event must survive a restart,
treat the broker as the subject and design each observer as an idempotent consumer. The dependency
direction is the same: the publisher knows the topic, not the subscribers.
