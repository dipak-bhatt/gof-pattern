using ObserverDesignPattern.Models;
using ObserverDesignPattern.Observers;
using ObserverDesignPattern.Services;
using ObserverDesignPattern.Subject;

Console.WriteLine("=== Observer Design Pattern — Order Status Notifications ===\n");
Console.WriteLine("SUBJECT    EventBus");
Console.WriteLine("OBSERVERS  confirmation-email · inventory · audit · analytics · loyalty-points");
Console.WriteLine("RULE       the publisher knows only the topic and the event");
Console.WriteLine("           it never holds a reference to an observer\n");

var bus = new EventBus();
var orders = new OrderService(bus);

var email = new ConfirmationEmailObserver();
var inventory = new InventoryObserver();
var audit = new AuditObserver();

Console.WriteLine("--- 1. Three observers attach themselves to the same topic ---");
var auditSubscription = bus.Subscribe(OrderTopics.StatusChanged, audit);
_ = bus.Subscribe(OrderTopics.StatusChanged, email);
_ = bus.Subscribe(OrderTopics.StatusChanged, inventory);
Console.WriteLine($"  {email.Name,-18} <- {OrderTopics.StatusChanged}");
Console.WriteLine($"  {inventory.Name,-18} <- {OrderTopics.StatusChanged}");
Console.WriteLine($"  {audit.Name,-18} <- {OrderTopics.StatusChanged}");

Console.WriteLine("\n--- 2. Place an order (Placed) ---");
Console.WriteLine($"  orders -> {OrderTopics.StatusChanged}");
await orders.PlaceAsync("ORD-1001", "Asha Rai", 4_200m);
Console.WriteLine("  note: e-mail and inventory act only when the order is Paid");

Console.WriteLine("\n--- 3. Pay the same order (Paid) ---");
Console.WriteLine($"  orders -> {OrderTopics.StatusChanged}");
await orders.PayAsync("ORD-1001", "Asha Rai", 4_200m);

Console.WriteLine("\n--- 4. Detach the audit observer; the publisher is unchanged ---");
auditSubscription.Dispose();
Console.WriteLine("  audit: unsubscribed");
Console.WriteLine($"  orders -> {OrderTopics.StatusChanged}");
await orders.PayAsync("ORD-1002", "Bikash Shrestha", 2_750m);
Console.WriteLine("  note: no audit line appears, but the other observers still run");

Console.WriteLine("\n--- 5. Attach a new observer while the program is running ---");
_ = bus.Subscribe(OrderTopics.StatusChanged, new AnalyticsObserver());
Console.WriteLine($"  analytics          <- {OrderTopics.StatusChanged}");
Console.WriteLine($"  orders -> {OrderTopics.StatusChanged}");
await orders.PlaceAsync("ORD-1003", "Sita Karki", 8_900m);

Console.WriteLine("\n--- 6. One observer fails; every other observer still receives the event ---");
_ = bus.Subscribe(OrderTopics.StatusChanged, new LoyaltyPointsObserver(failOnOrderId: "ORD-1004"));
Console.WriteLine($"  loyalty-points     <- {OrderTopics.StatusChanged}");
Console.WriteLine($"  orders -> {OrderTopics.StatusChanged}");
await orders.PayAsync("ORD-1004", "Hari Thapa", 6_300m);

Console.WriteLine("\n=== Key Insight ===");
Console.WriteLine("The publisher owns the event and the topic, nothing else. Each observer");
Console.WriteLine("attaches on its own and reacts on its own: detaching one removes only that");
Console.WriteLine("one, and adding another is a single Subscribe call. The order workflow and");
Console.WriteLine("every existing observer stay untouched.");
