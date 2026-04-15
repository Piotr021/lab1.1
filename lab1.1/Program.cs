using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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

        }
    }
}
    
