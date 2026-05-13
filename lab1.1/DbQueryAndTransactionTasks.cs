using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace lab1_1_net10
{
    public static class DbQueryAndTransactionTasks
    {
        public static async Task RunAsync(OrderFlowContext db)
        {
            Console.WriteLine("\n=== ZADANIE 3 — Zapytania LINQ i transakcje ===\n");

            // Uruchomienie zapytań LINQ na bazie danych
            await RunQueriesAsync(db);
            // Uruchomienie Transakcji
            await RunTransactionDemoAsync(db);
        }

        // Metoda zawierająca zapytania LINQ
        private static async Task RunQueriesAsync(OrderFlowContext db)
        {
            // Próg wartości zamówienia
            decimal vipThreshold = 1000m;

            // Pobranie zamówień klientów VIP powyżej podanego progu
            var vipOrdersQuery = db.Orders
                .AsNoTracking()
                .Where(o => o.Customer.IsVip)
                .Select(o => new
                {
                    o.Id,
                    CustomerName = o.Customer.Name,
                    o.Customer.City,
                    Total = o.Items.Sum(i => i.UnitPrice * i.Quantity),
                    o.Status
                })
                .Where(o => o.Total > vipThreshold)
                .OrderByDescending(o => o.Total);

            // Wykonanie zapytania i pobranie wyników z bazy
            var vipOrders = await vipOrdersQuery.ToListAsync();

            Console.WriteLine($"1) Zamówienia klientów VIP powyżej {vipThreshold:C}");
            foreach (var order in vipOrders)
            {
                Console.WriteLine($"#{order.Id} | {order.CustomerName} | {order.City} | {order.Total:C} | {order.Status}");
            }

            // Ranking klientów według łącznej wartości zamówień
            var customerRankingQuery = db.Orders
                .AsNoTracking()
                .GroupBy(o => new { o.CustomerId, o.Customer.Name })
                .Select(g => new
                {
                    CustomerName = g.Key.Name,
                    TotalValue = g.Sum(o => o.Items.Sum(i => i.UnitPrice * i.Quantity)),
                    OrdersCount = g.Count()
                })
                // Sortowanie od największej wartości zamówień
                .OrderByDescending(x => x.TotalValue);

            // Wykonanie zapytania i pobranie wyników z bazy
            var customerRanking = await customerRankingQuery.ToListAsync();

            Console.WriteLine("\n2) Ranking klientów wg łącznej wartości zamówień");
            foreach (var row in customerRanking)
            {
                Console.WriteLine($"{row.CustomerName} | zamówień: {row.OrdersCount} | suma: {row.TotalValue:C}");
            }

            // Średnia wartość zamówień dla każdego miasta
            var averageByCityQuery = db.Orders
                .AsNoTracking()
                .Select(o => new
                {
                    o.Customer.City,
                    Total = o.Items.Sum(i => i.UnitPrice * i.Quantity)
                })
                .GroupBy(x => x.City)
                .Select(g => new
                {
                    City = g.Key,
                    AverageOrderValue = g.Average(x => x.Total),
                    OrdersCount = g.Count()
                })
                .OrderByDescending(x => x.AverageOrderValue);

            // Wykonanie zapytania i pobranie wyników z bazy
            var averageByCity = await averageByCityQuery.ToListAsync();

            Console.WriteLine("\n3) Średnia wartość zamówienia per miasto klienta");
            foreach (var row in averageByCity)
            {
                Console.WriteLine($"{row.City} | średnia: {row.AverageOrderValue:C} | zamówień: {row.OrdersCount}");
            }

            // Produkty, które nigdy nie zostały zamówione
            var neverOrderedProductsQuery = db.Products
                .AsNoTracking()
                .Where(p => !p.OrderItems.Any())
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Stock
                });

            // Wykonanie zapytania i pobranie wyników z bazy
            var neverOrderedProducts = await neverOrderedProductsQuery.ToListAsync();

            Console.WriteLine("\n4) Produkty, które nigdy nie zostały zamówione");
            foreach (var product in neverOrderedProducts)
            {
                Console.WriteLine($"#{product.Id} | {product.Name} | {product.Category} | stock: {product.Stock}");
            }

            // Dynamiczne zapytanie IQueryable
            Console.WriteLine("\n5) Dynamiczne zapytanie IQueryable");
            Console.Write("Podaj status zamówienia, np. New/Processing/Completed, albo zostaw puste: ");
            var statusText = Console.ReadLine();

            Console.Write("Podaj minimalną kwotę zamówienia albo zostaw puste: ");
            var minTotalText = Console.ReadLine();

            // Bazowe zapytanie
            IQueryable<Order> dynamicQuery = db.Orders
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product);

            // Dodanie filtra po statusie jeśli użytkownik podał poprawną wartość
            if (Enum.TryParse<OrderStatus>(statusText, ignoreCase: true, out var statusFilter))
            {
                dynamicQuery = dynamicQuery.Where(o => o.Status == statusFilter);
            }

            // Dodanie filtra po minimalnej kwocie zamówienia
            if (decimal.TryParse(minTotalText, NumberStyles.Number, CultureInfo.CurrentCulture, out var minTotal))
            {
                dynamicQuery = dynamicQuery.Where(o => o.Items.Sum(i => i.UnitPrice * i.Quantity) >= minTotal);
            }

            // Wykonanie dynamicznie zbudowanego zapytania
            var dynamicResult = await dynamicQuery
                .Select(o => new
                {
                    o.Id,
                    CustomerName = o.Customer.Name,
                    o.Status,
                    Total = o.Items.Sum(i => i.UnitPrice * i.Quantity)
                })
                .OrderByDescending(o => o.Total)
                .ToListAsync();

            foreach (var order in dynamicResult)
            {
                Console.WriteLine($"#{order.Id} | {order.CustomerName} | {order.Status} | {order.Total:C}");
            }
        }

        // Metoda przetwarzająca zamówienie w transakcji
        public static async Task ProcessOrderAsync(OrderFlowContext db, int orderId)
        {
            // Rozpoczęcie transakcji
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                // Pobranie zamówienia razem z produktami
                var order = await db.Orders
                    .Include(o => o.Items)
                        .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                    throw new InvalidOperationException($"Nie znaleziono zamówienia #{orderId}.");

                if (order.Status != OrderStatus.New)
                    throw new InvalidOperationException($"Zamówienie #{orderId} ma status {order.Status}, a wymagany jest New.");

                // Zmiana statusu na Processing
                order.Status = OrderStatus.Processing;
                // Zapis zmian w bazie
                await db.SaveChangesAsync();

                foreach (var item in order.Items)
                {
                    // Jeśli brakuje produktu przerwij transakcję
                    if (item.Product.Stock < item.Quantity)
                    {
                        throw new InvalidOperationException(
                            $"Brak towaru: {item.Product.Name}. Wymagane: {item.Quantity}, dostępne: {item.Product.Stock}.");
                    }

                    item.Product.Stock -= item.Quantity;
                }

                // Zmiana statusu na Completed
                order.Status = OrderStatus.Completed;
                // Zapis zmian w bazie
                await db.SaveChangesAsync();

                // Zatwierdzenie transakcji
                await transaction.CommitAsync();
            }
            catch
            {
                // Cofnięcie wszystkich zmian jeśli wystąpił błąd
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Demonstracja udanej i nieudanej transakcji
        private static async Task RunTransactionDemoAsync(OrderFlowContext db)
        {
            Console.WriteLine("\n=== Transakcja procesowania zamówienia ===");

            // Pobranie przykładowego klienta i produktów
            var customer = await db.Customers.FirstAsync();
            var laptop = await db.Products.FirstAsync(p => p.Name == "Laptop");
            var headphones = await db.Products.FirstAsync(p => p.Name == "Headphones");

            // Ustawienie stanów magazynowych produktów
            laptop.Stock = 5;
            headphones.Stock = 1;
            // Zapisanie zmian w bazie
            await db.SaveChangesAsync();

            // Zamówienie które powinno zakończyć się sukcesem
            var successOrder = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Notes = "Demo transakcji — sukces",
                Items =
                {
                    new OrderItem
                    {
                        ProductId = laptop.Id,
                        Product = laptop,
                        Quantity = 2,
                        UnitPrice = laptop.Price
                    }
                }
            };

            // Zamówienie które powinno zakończyć się niepowodzeniem(rollbackiem)
            var failedOrder = new Order
            {
                CustomerId = customer.Id,
                OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Notes = "Demo transakcji — rollback",
                Items =
                {
                    new OrderItem
                    {
                        ProductId = headphones.Id,
                        Product = headphones,
                        Quantity = 5,
                        UnitPrice = headphones.Price
                    }
                }
            };

            // Zapisanie zamówień do bazy
            db.Orders.AddRange(successOrder, failedOrder);
            // Zapisanie zmian w bazie
            await db.SaveChangesAsync();

            // Zapisanie ID utworzonych zamówień
            var successOrderId = successOrder.Id;
            var failedOrderId = failedOrder.Id;

            // Usunięcie zapisanych obiektów z pamięci EF Core
            db.ChangeTracker.Clear();

            Console.WriteLine($"\nScenariusz sukcesu: zamówienie #{successOrderId}");
            // Próba poprawnego przetworzenia zamówienia
            await ProcessOrderAsync(db, successOrderId);
            // Wyświetlenie statusu zamówienia i aktualnego stocku po udanej transakcji
            await PrintOrderAndStockAsync(db, successOrderId, laptop.Id);

            Console.WriteLine($"\nScenariusz niepowodzenia: zamówienie #{failedOrderId}");
            try
            {
                // Próba przetworzenia zamówienia z brakującym produktem
                await ProcessOrderAsync(db, failedOrderId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Rollback wykonany. Powód: {ex.Message}");
                // Usunięcie zapisanych obiektów z pamięci EF Core
                db.ChangeTracker.Clear();
            }

            // Wyświetlenie statusu zamówienia i stocku po rollbacku
            await PrintOrderAndStockAsync(db, failedOrderId, headphones.Id);
        }

        // Wyświetlenie statusu zamówienia i aktualnego stocku produktu
        private static async Task PrintOrderAndStockAsync(OrderFlowContext db, int orderId, int productId)
        {
            // Pobranie aktualnych danych z bazy
            var order = await db.Orders.AsNoTracking().FirstAsync(o => o.Id == orderId);
            var product = await db.Products.AsNoTracking().FirstAsync(p => p.Id == productId);

            Console.WriteLine($"Zamówienie #{order.Id} | status: {order.Status}");
            Console.WriteLine($"Produkt {product.Name} | aktualny stock: {product.Stock}");
        }
    }
}