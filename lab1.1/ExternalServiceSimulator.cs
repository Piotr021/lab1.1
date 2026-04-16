using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    public class ExternalServiceSimulator
    {
        // Readonly referencja nie może być zmieniona po inicjalizacji
        private static readonly Random _random = new Random();
        // Lock do synchronizacji dostępu do obiektu Random, bez tego mogą wystąpić problemy z wielowątkowością 
        // i poprawnym generowaniem liczb losowych, ponieważ Random nie jest bezpieczny dla wątków
        private static readonly object _lock = new object();

        // Losowanie liczby z zakresu min-max
        private int NextRandom(int min, int max)
        {
            lock (_lock)
            {
                return _random.Next(min, max + 1);
            }
        }

        // Task - nie wiadomo, kiedy zakończy się operacja, ale będzie zwracać bool
        public async Task<bool> CheckInventoryAsync(Product product)
        {
            // Losowanie czasu
            int delay = NextRandom(500, 1500);
            // Czekanie symulujące opóźnienie zewnętrznej usługi
            await Task.Delay(delay);

            // Losowanie dostępności produktu
            // Jeśli wylosowana liczba jest większa niż 30, to produkt jest dostępny (true)
            bool available = NextRandom(0, 100) > 30;
            Console.WriteLine(
                $"[Inventory] Produkt: {product.Name,-15} | dostępny: {available,-5} | czas: {delay} ms");

            return available;
        }

        // Task - nie wiadomo, kiedy zakończy się operacja, ale będzie zwracać bool
        public async Task<bool> ValidatePaymentAsync(Order order)
        {
            // Losowanie czasu
            int delay = NextRandom(1000, 2000);
            // Czekanie symulujące opóźnienie zewnętrznej usługi
            await Task.Delay(delay);

            // Symulacja akceptacji płatności
            // Jeśli wylosowana liczba jest większa niż 5, to płatność jest zaakceptowana (true)
            bool paymentAccepted = NextRandom(0, 100) > 5;
            Console.WriteLine(
                $"[Payment] Zamówienie #{order.Id} | płatność OK: {paymentAccepted,-5} | czas: {delay} ms");

            return paymentAccepted;
        }

        // Task - nie wiadomo, kiedy zakończy się operacja, ale będzie zwracać decimal
        public async Task<decimal> CalculateShippingAsync(Order order)
        {
            // Losowanie czasu
            int delay = NextRandom(300, 800);
            // Czekanie symulujące opóźnienie zewnętrznej usługi
            await Task.Delay(delay);

            // Obliczanie kosztu wysyłki: darmowa dla zamówień powyżej 1000, w przeciwnym razie 25 + 2 za każdą sztukę
            decimal shipping = order.TotalAmount >= 1000m ? 0m : 25m + order.Items.Sum(i => i.Quantity * 2m);
            Console.WriteLine(
                $"[Shipping] Zamówienie #{order.Id} | koszt wysyłki: {shipping:C} | czas: {delay} ms");

            return shipping;
        }

        // Metoda asynchroniczna bez wyniku
        public async Task ProcessOrderAsync(Order order)
        {
            // Sprawdzenie, czy zamówienie istnieje (nie jest null)
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            Console.WriteLine($"\n--- Start przetwarzania zamówienia #{order.Id} ---");

            // Stopwatch do pomiaru czasu potrzebnego do przetwarzania zamówienia
            var stopwatch = Stopwatch.StartNew();

            // Tworzymy listę zadań dla każdego produktu w zamówieniu, które sprawdzają dostępność w magazynie
            var inventoryTasks = order.Items
                .Select(i => CheckInventoryAsync(i.Product))
                .ToList();

            // Tworzymy zadanie dla walidacji płatności
            var paymentTask = ValidatePaymentAsync(order);
            // Tworzymy zadanie dla obliczenia kosztu wysyłki
            var shippingTask = CalculateShippingAsync(order);

            // Czekamy na zakończenie wszystkich zadań (magazyn, płatność, wysyłka)
            await Task.WhenAll(inventoryTasks.Cast<Task>()
                .Append(paymentTask)
                .Append(shippingTask));
            // Sprawdzamy czy wszystkie produkty są dostępne
            bool inventoryOk = inventoryTasks.All(t => t.Result);
            // Sprawdzamy czy płatność się powiodła
            bool paymentOk = paymentTask.Result;
            // Sprawdzamy koszt wysyłki
            decimal shippingCost = shippingTask.Result;

            // Zatrzymujemy stoper, aby zmierzyć całkowity czas przetwarzania zamówienia
            stopwatch.Stop();

            Console.WriteLine($"Wynik zamówienia #{order.Id}:");
            Console.WriteLine($"- Magazyn OK: {inventoryOk}");
            Console.WriteLine($"- Płatność OK: {paymentOk}");
            Console.WriteLine($"- Koszt wysyłki: {shippingCost:C}");
            Console.WriteLine($"- Czas łączny: {stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"--- Koniec zamówienia #{order.Id} ---\n");
        }

        // Metoda asynchroniczna bez wyniku
        public async Task ProcessMultipleOrdersAsync(List<Order> orders)
        {
            // Sprawdzenie, czy lista zamówień istnieje (nie jest null)
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            // SemaphoreSlim ograniczający liczbę jednocześnie przetwarzanych zamówień do 3
            using var semaphore = new SemaphoreSlim(3);
            // Licznik przetworzonych zamówień
            int processedCount = 0;
            // Całkowita liczba zamówień do przetworzenia
            int total = orders.Count;
            // Tworzymy zadania dla każdego zamówienia
            var tasks = orders.Select(async order =>
            {
                // Czekamy na dostęp ze względu na ograniczenia semafora
                await semaphore.WaitAsync();
                try
                {
                    // Przetwarzamy zamówienie
                    await ProcessOrderAsync(order);
                    // Bezpieczne narzędzie inkrementacji w środowisku wielowątkowym
                    int done = Interlocked.Increment(ref processedCount);
                    Console.WriteLine($"Przetworzono {done}/{total} zamówień");
                }
                finally
                {
                    // Zwalniamy semafor, aby inne zadania mogły się rozpocząć
                    semaphore.Release();
                }
            });
            // Czekamy na zakończenie wszystkich zadań
            await Task.WhenAll(tasks);
        }

        public async Task CompareSequentialVsParallelAsync(List<Order> orders)
        {
            // Sprawdzenie, czy lista zamówień istnieje (nie jest null)
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            Console.WriteLine("\n==============================");
            Console.WriteLine("PORÓWNANIE CZASU");
            Console.WriteLine("==============================");

            // sequentialWatch do pomiaru czasu potrzebnego do przetwarzania zamówień sekwencyjnie
            var sequentialWatch = Stopwatch.StartNew();
            foreach (var order in orders)
            {
                await ProcessOrderAsync(order);
            }
            // Zatrzymujemy stoper, aby zmierzyć całkowity czas przetwarzania zamówień sekwencyjnie
            sequentialWatch.Stop();

            Console.WriteLine($"Czas sekwencyjny: {sequentialWatch.ElapsedMilliseconds} ms");

            // parallelWatch do pomiaru czasu potrzebnego do przetwarzania zamówień równolegle (max 3 jednocześnie)
            var parallelWatch = Stopwatch.StartNew();
            await ProcessMultipleOrdersAsync(orders);
            // Zatrzymujemy stoper, aby zmierzyć całkowity czas przetwarzania zamówień równolegle
            parallelWatch.Stop();

            Console.WriteLine($"Czas równoległy (max 3): {parallelWatch.ElapsedMilliseconds} ms");
        }
    }
}