using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace lab1_1_net10
{
    public class Orderstatistics2
    {
        private int _totalProcessed;
        private decimal _totalRevenue;

        // Readonly referencja nie może być zmieniona po inicjalizacji
        private readonly object _revenueLock = new();
        private readonly object _errorsLock = new();

        // Lista tylko do odczytu do przechowywania błędów przetwarzania zamówień
        private readonly List<string> _processingErrors = new();

        // Właściwości tylko do odczytu(getter)
        public int TotalProcessed => _totalProcessed;
        public decimal TotalRevenue => _totalRevenue;

        // ConcurrentDictionary - słownik bezpieczny dla wątków, pozwala na aktualizację wartości bez konieczności stosowania zewnętrznych blokad
        public ConcurrentDictionary<OrderStatus, int> OrdersPerStatus { get; } = new();

        // IReadOnlyList - tylko do odczytu, nie można modyfikować z zewnątrz, ale wewnątrz klasy można aktualizować listę _processingErrors
        public IReadOnlyList<string> ProcessingErrors
        {
            get
            {
                lock (_errorsLock)
                {
                    return _processingErrors.ToList();
                }
            }
        }

        // -----------------------------
        // WERSJA BEZ SYNCHRONIZACJI
        // -----------------------------
        // Metoda bez synchronizacji do aktualizacji statystyk zamówienia
        public void UpdateUnsafe(Order order, bool isValid, List<string> errors)
        {
            // Liczba przetworzonych zamówień
            int processedSnapshot = _totalProcessed;
            Thread.SpinWait(5000);
            _totalProcessed = processedSnapshot + 1;

            if (isValid)
            {
                // Łączna kwota zamówień
                decimal revenueSnapshot = _totalRevenue;
                Thread.SpinWait(5000);
                _totalRevenue = revenueSnapshot + order.TotalAmount;

                IncrementStatusUnsafe(order.Status);
            }
            else
            {
                foreach (var error in errors)
                {
                    // Dodawanie komunnikatów błędów do listy
                    _processingErrors.Add($"Order #{order.Id}: {error}");
                }
            }
        }

        // Metoda bez synchronizacji do aktualizacji liczby zamówień dla danego statusu
        private void IncrementStatusUnsafe(OrderStatus status)
        {
            // Jeśli istnieje w słowniku wartość dla klucza status, zapisz wartość w current
            if (OrdersPerStatus.TryGetValue(status, out int current))
            {
                Thread.SpinWait(5000);
                OrdersPerStatus[status] = current + 1;
            }
            else
            {
                Thread.SpinWait(5000);
                OrdersPerStatus[status] = 1;
            }
        }

        // -----------------------------
        // WERSJA POPRAWNA
        // -----------------------------
        // Metoda z synchronizacją do aktualizacji statystyk zamówienia
        public void UpdateSafe(Order order, bool isValid, List<string> errors)
        {
            // Bezpieczna dla wielowątkowości inkrementacja liczby przetworzonych zamówień 
            Interlocked.Increment(ref _totalProcessed);

            if (isValid)
            {
                // Zabezpieczenie dostępu do wspólnego zasobu _totalRevenue
                // Tylko jeden wątek może modyfikować _totalRevenue w danym momencie
                lock (_revenueLock)
                {
                    // Aktualizacja łącznej kwoty zamówień
                    _totalRevenue += order.TotalAmount;
                }
                // Bezpieczna aktualizacja liczby zamówień dla danego statusu
                OrdersPerStatus.AddOrUpdate(order.Status, 1, (_, current) => current + 1);
            }
            else
            {
                // Zabezpieczenie dostępu do wspólnego zasobu _processingErrors
                lock (_errorsLock)
                {
                    foreach (var error in errors)
                    {
                        // Dodawanie komunnikatów błędów do listy
                        _processingErrors.Add($"Order #{order.Id}: {error}");
                    }
                }
            }
        }

        public string ToComparableSnapshot()
        {
            string statuses = string.Join(", ",
                OrdersPerStatus
                    // Sortowanie po kluczu (statusie)
                    .OrderBy(x => x.Key)
                    // Tworzenie stringa w formacie "Status=Count" dla każdego wpisu(klucz+wartość) w słowniku
                    // status : count -> "Status=Count"
                    .Select(x => $"{x.Key}={x.Value}"));

            return $"Processed={TotalProcessed}; Revenue={TotalRevenue}; Statuses=[{statuses}]; Errors={ProcessingErrors.Count}";
        }

        public void Print(string title)
        {
            Console.WriteLine($"\n===== {title} =====");
            Console.WriteLine($"TotalProcessed: {TotalProcessed}");
            Console.WriteLine($"TotalRevenue: {TotalRevenue:C}");

            Console.WriteLine("OrdersPerStatus:");
            foreach (var kv in OrdersPerStatus.OrderBy(x => x.Key))
            {
                Console.WriteLine($"- {kv.Key}: {kv.Value}");
            }

            Console.WriteLine($"ProcessingErrors: {ProcessingErrors.Count}");
            foreach (var err in ProcessingErrors.Take(5))
            {
                Console.WriteLine($"  * {err}");
            }

            if (ProcessingErrors.Count > 5)
                Console.WriteLine("  ...");
        }
    }
}