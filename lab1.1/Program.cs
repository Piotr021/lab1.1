using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace lab1_1_net10
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            var validator = new OrderValidator();
            Console.WriteLine("Walidacja zamówień\n");
            foreach (var order in SampleData.Orders)
            {
                var errors = validator.ValidateAll(order);

                Console.WriteLine($"Order {order.Id}");

                if (errors.Count == 0)
                    Console.WriteLine("OK");
                else
                    errors.ForEach(e => Console.WriteLine($"- {e}"));

                Console.WriteLine();
            }

            var orders = SampleData.Orders;
            var processor = new OrderProcessor(orders);

            Console.WriteLine("=== ZADANIE 3 - Action, Func, Predicate ===\n");

            // 1. Predicate<Order> — minimum 3 różne predykaty jako lambdy

            Predicate<Order> highValueOrders = o => o.TotalAmount > 500m;
            Predicate<Order> completedOrders = o => o.Status == OrderStatus.Completed;
            Predicate<Order> vipCustomerOrders = o => o.Customer.IsVip;

            Console.WriteLine("=== Zamówienia powyżej 500 zł ===");
            var expensiveOrders = processor.FilterOrders(highValueOrders);
            foreach (var order in expensiveOrders)
            {
                Console.WriteLine($"Zamówienie #{order.Id}, klient: {order.Customer.Name}, kwota: {order.TotalAmount:C}, status: {order.Status}");
            }

            Console.WriteLine("\n=== Zamówienia zakończone ===");
            var doneOrders = processor.FilterOrders(completedOrders);
            foreach (var order in doneOrders)
            {
                Console.WriteLine($"Zamówienie #{order.Id}, data: {order.OrderDate:d}, kwota: {order.TotalAmount:C}");
            }

            Console.WriteLine("\n=== Zamówienia klientów VIP ===");
            var vipOrders = processor.FilterOrders(vipCustomerOrders);
            foreach (var order in vipOrders)
            {
                Console.WriteLine($"Zamówienie #{order.Id}, klient VIP: {order.Customer.Name}, kwota: {order.TotalAmount:C}");
            }

            // 2. Action<Order> — minimum 2 zastosowania

            Console.WriteLine("\n=== Action #1: wypisywanie zamówień ===");
            Action<Order> printOrder = o =>
                Console.WriteLine($"[PRINT] #{o.Id} | {o.Customer.Name} | {o.Status} | {o.TotalAmount:C}");

            processor.ProcessOrders(orders.Take(3), printOrder);

            Console.WriteLine("\n=== Action #2: zmiana statusu New -> Processing ===");
            Action<Order> changeStatus = o =>
            {
                if (o.Status == OrderStatus.New)
                {
                    o.Status = OrderStatus.Processing;
                    Console.WriteLine($"Zmieniono status zamówienia #{o.Id} na {o.Status}");
                }
            };

            processor.ProcessOrders(orders, changeStatus);

            // 3. Func<Order, T> — projekcja na dowolny typ

            Console.WriteLine("\n=== Projekcja do typu anonimowego ===");
            var projectedOrders = processor.ProjectOrders(o => new
            {
                OrderNumber = o.Id,
                CustomerName = o.Customer.Name,
                Amount = o.TotalAmount,
                ItemCount = o.Items.Sum(i => i.Quantity),
                IsVip = o.Customer.IsVip
            });

            foreach (var item in projectedOrders)
            {
                Console.WriteLine(
                    $"Nr: {item.OrderNumber}, Klient: {item.CustomerName}, Kwota: {item.Amount:C}, " +
                    $"Liczba sztuk: {item.ItemCount}, VIP: {item.IsVip}");
            }

            // 4. Agregacja — minimum 3 agregatory

            // sumowanie wartości wszystkich zamówień
            decimal totalAmount = processor.AggregateOrders(os => os.Sum(o => o.TotalAmount));
            // os.any czy lista zawiera coś, jeśli tak = średnia z sumy kosztów zamówień, jeśli nie to 0
            decimal averageAmount = processor.AggregateOrders(os => os.Any() ? os.Average(o => o.TotalAmount) : 0m);
            // os.any czy lista zawiera coś, jeśli tak = maxymalna wartość wśród zamówień, jeśli nie to 0
            decimal maxAmount = processor.AggregateOrders(os => os.Any() ? os.Max(o => o.TotalAmount) : 0m);

            Console.WriteLine("\n=== Agregacje ===");
            Console.WriteLine($"Suma wszystkich zamówień: {totalAmount:C}");
            Console.WriteLine($"Średnia wartość zamówienia: {averageAmount:C}");
            Console.WriteLine($"Największe zamówienie: {maxAmount:C}");

            // 5. Łańcuch: filtruj -> sortuj -> weź top N -> wypisz

            Console.WriteLine("\n=== Flow: filtruj -> sortuj -> top N -> wypisz ===");

            Predicate<Order> chainPredicate = o =>
                o.Status != OrderStatus.Cancelled && o.TotalAmount < 7000m;

            Func<Order, object> sortBy = o => o.TotalAmount;

            Action<Order> chainPrint = o =>
                Console.WriteLine($"TOP | #{o.Id} | {o.Customer.Name} | {o.TotalAmount:C} | {o.Status}");

            processor.FilterSortTakeAndPrint(
                chainPredicate,
                sortBy,
                3,
                chainPrint
            );

            Console.WriteLine("\n=== Koniec zadania 3 ===");
            LinqTasks.Run();

            Console.WriteLine("\n=== Koniec zadania 4 ===");
            Console.WriteLine("\nNaciśnij dowolny klawisz, aby zakończyć...");
            Console.ReadKey();


            var pipeline = new OrderPipeline();

            var logger = new ConsoleLogger();
            var email = new EmailNotifier();
            var stats = new OrderStatistics();

            pipeline.ValidationCompleted += logger.OnValidationCompleted;
            pipeline.StatusChanged += logger.OnStatusChanged;

            pipeline.StatusChanged += email.OnStatusChanged;

            pipeline.ValidationCompleted += stats.OnValidationCompleted;
            pipeline.StatusChanged += stats.OnStatusChanged;

            foreach (var order in SampleData.OrdersForPipeline)
            {
                pipeline.ProcessOrder(order);
            }

            stats.PrintStatistics();

            Console.WriteLine("\n=== ZADANIE 2 - Asynchroniczne pobieranie danych ===\n");

            var externalService = new ExternalServiceSimulator();

            // bierzemy przykładowe zamówienia z danych
            var asyncOrders = SampleData.OrdersForPipeline.Take(3).ToList();

            await externalService.CompareSequentialVsParallelAsync(asyncOrders);

            Console.WriteLine("\n=== Koniec zadania 2 ===");



            RunThreadSafetyDemo();

            Console.WriteLine("\n=== Koniec zadania 3 ===");


            // tworzenie obiektu do zarządzania zamówieniami i ich zapisem/odczytem
            var repository = new OrderRepository();

            // ścieżki do plików
            const string jsonPath = "data/orders.json";
            const string xmlPath = "data/orders.xml";

            // dane z sample data
            var originalOrders = SampleData.Orders.ToList();

            Console.WriteLine("=== ZADANIE 1 - Repozytorium JSON/XML ===\n");

            Console.WriteLine($"Liczba oryginalnych zamówień: {originalOrders.Count}");
            Console.WriteLine($"Suma oryginalnych kwot: {originalOrders.Sum(o => o.TotalAmount):C}");

            // zapis do plików json i xml
            await repository.SaveToJsonAsync(originalOrders, jsonPath);
            await repository.SaveToXmlAsync(originalOrders, xmlPath);

            // czyszczenie pamięci
            originalOrders = new List<Order>();

            // wczytywanie danych z plików json i xml
            var loadedFromJson = await repository.LoadFromJsonAsync(jsonPath);
            var loadedFromXml = await repository.LoadFromXmlAsync(xmlPath);

            Console.WriteLine("\n--- JSON ---");
            Console.WriteLine($"Wczytano zamówień: {loadedFromJson.Count}");
            Console.WriteLine($"Suma kwot: {loadedFromJson.Sum(o => o.TotalAmount):C}");

            Console.WriteLine("\n--- XML ---");
            Console.WriteLine($"Wczytano zamówień: {loadedFromXml.Count}");
            Console.WriteLine($"Suma kwot: {loadedFromXml.Sum(o => o.TotalAmount):C}");

            // porównanie wyników z oryginalnymi danymi
            // liczba zamówień
            var originalCount = SampleData.Orders.Count;
            // suma kwot zamówień
            var originalSum = SampleData.Orders.Sum(o => o.TotalAmount);

            // porównanie liczby zamówień i sumy kwot dla danych wczytanych z json i xml z oryginalnymi danymi
            bool jsonOk = loadedFromJson.Count == originalCount
                          && loadedFromJson.Sum(o => o.TotalAmount) == originalSum;

            bool xmlOk = loadedFromXml.Count == originalCount
                         && loadedFromXml.Sum(o => o.TotalAmount) == originalSum;

            Console.WriteLine("\n--- PORÓWNANIE ---");
            Console.WriteLine($"JSON round-trip OK: {jsonOk}");
            Console.WriteLine($"XML round-trip OK: {xmlOk}");
            Console.WriteLine();

            Console.WriteLine("\n=== Koniec zadania 1 ===");

            // Wczytywanie danych z sample data
            var orders2 = SampleData.Orders;

            // Obiekt do budowania raportu XML
            var reportBuilder = new XmlReportBuilder();

            // Budowanie raportu na podstawie zamówień
            var report = reportBuilder.BuildReport(orders2);

            // Zapis raportu do pliku
            await reportBuilder.SaveReportAsync(
                report,
                "data/report.xml");

            Console.WriteLine(
                "Raport XML został zapisany do data/report.xml");

            // Asynchroniczne pobieranie identyfikatorów zamówień o wartości powyżej 1000 zł z raportu XML
            var highValueOrders2 = await reportBuilder
                .FindHighValueOrderIdsAsync(
                    "data/report.xml",
                    1000m);

            Console.WriteLine();
            Console.WriteLine("Zamówienia powyżej 1000:");

            foreach (var orderId in highValueOrders2)
            {
                Console.WriteLine($"Order ID: {orderId}");
            }

            Console.WriteLine("\n=== Koniec zadania 2 ===");

            // Obiekty do walidacji zamówień
            var validator2 = new OrderValidator();
            // Obiekty wypisujące informacje w konsoli
            var logger2 = new ConsoleLogger();
            // Obiekt symulujący wysyłania powiadomień email
            var notifier = new EmailNotifier();

            // Obiekt do przetwarzania zamówień
            var pipeline2 = new OrderPipeline();

            // Reakcja na zmianę statusu zamówienia - wypisanie informacji w konsoli
            pipeline2.StatusChanged += (sender, e) =>
            {
                Console.WriteLine(
                    $"STATUS: Zamówienie {e.Order.Id} -> {e.NewStatus}");
            };

            // Ścieżka do folderu "inbox" (aktualny folder działania programu + inbox)
            var inboxPath = Path.Combine(
                Directory.GetCurrentDirectory(),
                "inbox");

            // Obiekt do monitorowania folderu "inbox"
            using var watcher = new InboxWatcher(inboxPath, pipeline2);

            Console.WriteLine("Program działa...");
            Console.WriteLine("Tworzenie testowych plików co 3 sekundy...\n");


            for (int i = 1; i <= 3; i++)
            {
                // Pobieranie zamówienia z danych
                var orders3 = SampleData.OrdersForPipeline;

                // Tworzenie unikalnej nazwy pliku
                var fileName = $"orders_{DateTime.Now:HHmmss}_{i}.json";

                // Pełna ścieżka do pliku w folderze "inbox"
                var path = Path.Combine(inboxPath, fileName);

                // Obiekt do zarządzania zamówieniami i ich zapisem/odczytem
                var repository2 = new OrderRepository();

                // Zapis zamówień do pliku JSON
                await repository2.SaveToJsonAsync(orders3, path);

                Console.WriteLine($"Utworzono plik: {fileName}");

                // Odczekaj 3 sekundy przed utworzeniem kolejnego pliku
                await Task.Delay(3000);
            }

            Console.WriteLine("\n=== Koniec zadania 3 ===");
            // BAZA ----------------------------------------------------------------------------------------------------------

            // Połączenie programu z bazą danych
            using var db = new OrderFlowContext();

            // Utworzenie / aktualizacja bazy danych na podstawie migracji
            await db.Database.MigrateAsync();

            // Wypełnienie bazy danymi z SampleData
            await DatabaseSeeder.SeedAsync(db);

            await DbQueryAndTransactionTasks.RunAsync(db);

            Console.WriteLine("\nCRUD\n");

            // CREATE

            // Pobranie istniejącego klienta i produktów z bazy
            var existingCustomer = await db.Customers.FirstAsync();
            // Pobranie pierwszego produktu
            var product1 = await db.Products.FirstAsync();
            // Pobranie drugiego produktu
            var product2 = await db.Products.Skip(1).FirstAsync();

            // Utworzenie nowego zamówienia z 2 pozycjami
            var newOrder = new Order
            {
                CustomerId = existingCustomer.Id,
                Customer = existingCustomer,
                OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Notes = "Nowe zamówienie CRUD",
                Items = new List<OrderItem>
    {
        new OrderItem
    {
        ProductId = product1.Id,
        Product = product1,
        Quantity = 1,
        UnitPrice = product1.Price
    },
    new OrderItem
    {
        ProductId = product2.Id,
        Product = product2,
        Quantity = 2,
        UnitPrice = product2.Price
    }
    }
            };

            // Przekazanie zamówienia do zapisania
            db.Orders.Add(newOrder);

            // Zapisanie danych do bazy
            await db.SaveChangesAsync();

            Console.WriteLine($"Dodano zamówienie #{newOrder.Id} z 2 pozycjami.");

            // READ

            // Pobranie zamówień razem z klientem oraz produktami pozycji zamówienia
            var orders4 = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .ToListAsync();

            Console.WriteLine("\n=== Lista zamówień ===");

            foreach (var order in orders4)
            {
                Console.WriteLine(
                    $"\nZamówienie #{order.Id} | Klient: {order.Customer.Name} | Status: {order.Status}");

                foreach (var item in order.Items)
                {
                    Console.WriteLine(
                        $"- {item.Product.Name} | Ilość: {item.Quantity} | Cena: {item.UnitPrice:C}");
                }
            }

            // UPDATE

            // Pobranie pierwszego zamówienia ze statusem New
            var orderToUpdate = await db.Orders
                // Pobranie pierwszego zamówienia ze statusem New inaczej null
                .FirstOrDefaultAsync(o => o.Status == OrderStatus.New);

            if (orderToUpdate != null)
            {
                // Zmiana statusu i notatki
                orderToUpdate.Status = OrderStatus.Processing;
                orderToUpdate.Notes = "Status zmieniony podczas CRUD demo";

                // Zapisanie zmian
                await db.SaveChangesAsync();

                Console.WriteLine(
                    $"\nZmieniono status zamówienia #{orderToUpdate.Id} na {orderToUpdate.Status}.");
            }

            // DELETE

            // Pobranie anulowanego zamówienia
            var cancelledOrder = await db.Orders
                // Pobranie pierwszego zamówienia ze statusem Cancelled inaczej null
                .FirstOrDefaultAsync(o => o.Status == OrderStatus.Cancelled);

            if (cancelledOrder != null)
            {
                // Usunięcie zamówienia
                db.Orders.Remove(cancelledOrder);

                // Zapisanie zmian
                await db.SaveChangesAsync();

                Console.WriteLine($"\nUsunięto zamówienie #{cancelledOrder.Id}.");
            }
            else
            {
                Console.WriteLine("\nBrak anulowanego zamówienia do usunięcia.");
            }

            Console.WriteLine("Naciśnij ENTER aby zakończyć...");
            Console.ReadLine();

        }
        private static void RunThreadSafetyDemo()
        {
            Console.WriteLine("\n=== ZADANIE 3 - Thread safety ===\n");

            // const - stała wartość
            // ile razy powielić dane testowe
            const int repeatCount = 4000;
            // ile razy uruchomić test
            const int runs = 5;

            // tworzy liste zamówień testowych
            var expectedOrders = SampleData.CreateOrdersForStatisticsDemo(repeatCount);
            // oblicza oczekiwany wynik na podstawie tych zamówień
            string expected = CalculateExpectedSnapshot(expectedOrders);

            Console.WriteLine("Oczekiwany wynik:");
            Console.WriteLine(expected);
            Console.WriteLine();

            Console.WriteLine("----- WERSJA UNSAFE -----");

            for (int i = 1; i <= runs; i++)
            {
                // lista zamówień testowych
                var orders = SampleData.CreateOrdersForStatisticsDemo(repeatCount);
                // obiekt do zbierania statystyk
                var stats = new Orderstatistics2();
                // obiekt do sprawdzania, czy zamówienie jest poprawne
                var validator = new OrderValidator();

                // przejdź po wszystkich zamówieniach, rób to równolegle, na wielu wątkach
                Parallel.ForEach(orders, order =>
                {
                    // sprawdź, czy zamówienie jest poprawne, zbierz błędy
                    var errors = validator.ValidateAll(order);
                    // jeśli nie ma błędów, to jest poprawne
                    bool isValid = errors.Count == 0;

                    if (isValid)
                        order.Status = OrderStatus.Completed;
                    // zaktualizuj statystyki, w sposób niebezpieczny (bez synchronizacji)
                    stats.UpdateUnsafe(order, isValid, errors);
                });
                // wypisz wynik, porównaj z oczekiwanym
                Console.WriteLine($"Run {i}: {stats.ToComparableSnapshot()}");
            }

            Console.WriteLine();
            Console.WriteLine("----- WERSJA SAFE -----");

            for (int i = 1; i <= runs; i++)
            {
                // lista zamówień testowych
                var orders = SampleData.CreateOrdersForStatisticsDemo(repeatCount);
                // obiekt do zbierania statystyk
                var stats = new Orderstatistics2();
                // obiekt do sprawdzania, czy zamówienie jest poprawne
                var validator = new OrderValidator();

                // przejdź po wszystkich zamówieniach, rób to równolegle, na wielu wątkach
                Parallel.ForEach(orders, order =>
                {

                    // sprawdź, czy zamówienie jest poprawne, zbierz błędy
                    var errors = validator.ValidateAll(order);
                    // jeśli nie ma błędów, to jest poprawne
                    bool isValid = errors.Count == 0;

                    if (isValid)
                        order.Status = OrderStatus.Completed;

                    // zaktualizuj statystyki, w sposób bezpieczny (z synchronizacją)
                    stats.UpdateSafe(order, isValid, errors);
                });
                // wypisz wynik, porównaj z oczekiwanym
                Console.WriteLine($"Run {i}: {stats.ToComparableSnapshot()}");
            }
        }

            private static string CalculateExpectedSnapshot(List<Order> orders)
        {
            // obiekt do sprawdzania, czy zamówienie jest poprawne
            var validator = new OrderValidator();
            // liczba wszystkich zamówień
            int totalProcessed = orders.Count;
            // suma wartości zamówień
            decimal totalRevenue = 0m;
            // liczba zamówień poprawnych,zakończonych
            int completed = 0;
            // liczba błędów
            int errorsCount = 0;

            foreach (var order in orders)
            {
                // sprawdź, czy zamówienie jest poprawne, zbierz błędy
                var errors = validator.ValidateAll(order);

                if (errors.Count == 0)
                {
                    totalRevenue += order.TotalAmount;
                    completed++;
                }
                else
                {
                    errorsCount += errors.Count;
                }
            }

            return $"Processed={totalProcessed}; Revenue={totalRevenue}; Statuses=[Completed={completed}]; Errors={errorsCount}";



        }
    }
}
    
