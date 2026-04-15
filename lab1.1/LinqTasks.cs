using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace lab1_1_net10
{
    public static class LinqTasks
    {
        public static void Run()
        {
            var products = SampleData.Products;
            var customers = SampleData.Customers;
            var orders = SampleData.Orders;

            Console.WriteLine("==================================================");
            Console.WriteLine("ZADANIE 4 - LINQ");
            Console.WriteLine("==================================================");

            // 1. JOIN + GROUP BY (query syntax)
            // Zapytanie łączy zamówienia z klientami i odpowiednio je grupuje
            // Czy klient jest VIP-em ?
            // Query syntax bo przy join + group jest bardzo czytelna i przypomina składnię SQL
            var ordersByVip =
                from order in orders
                // Dołącza klienta do zamówienia na podstawie zgodnego Id
                // Tworząc obiekt anonimowy - tymczasowe pary (order, customer) używane dalej w zapytaniu
                join customer in customers on order.Customer.Id equals customer.Id
                // Dzieli zamówienia na grupy w zależności od tego, czy klient jest VIP-em
                group order by customer.IsVip into vipGroup
                // Tworzy nowy obiekt anonimowy dla każdej grupy, zawierający informację o statusie VIP, liczbie zamówień i sumie wartości
                select new
                {
                    IsVip = vipGroup.Key,
                    OrdersCount = vipGroup.Count(),
                    TotalAmount = vipGroup.Sum(o => o.TotalAmount)
                };

            Console.WriteLine("\n1. Zamówienia pogrupowane według statusu VIP (JOIN + GROUP BY, query syntax) :");
            foreach (var item in ordersByVip)
            {
                Console.WriteLine($"VIP: {item.IsVip,-5} | Liczba zamówień: {item.OrdersCount,-3} | Suma: {item.TotalAmount:C}");
            }

            // 2. JOIN (method syntax)
            // To zapytanie tworzy prosty raport zamówień z nazwą klienta
            // Method syntax jasno pokazuje co dzieje się krok po kroku
            var ordersWithCustomers = orders
                .Join(
                    // Łączy w parę zamówienia z klientami
                    customers,
                    // porównuje id klienta w zamówieniu z id klienta w liście klientów
                    order => order.Customer.Id,
                    customer => customer.Id,
                    // tworzy nowy obiekt anonimowy łączący dane
                    (order, customer) => new
                    {
                        OrderId = order.Id,
                        CustomerName = customer.Name,
                        customer.IsVip,
                        order.Status,
                        order.TotalAmount
                    });

            Console.WriteLine("\n2. Lista zamówień z klientami (JOIN, method syntax):");
            foreach (var item in ordersWithCustomers)
            {
                Console.WriteLine($"Zamówienie #{item.OrderId} | {item.CustomerName,-20} | VIP: {item.IsVip,-5} | {item.Status,-12} | {item.TotalAmount:C}");
            }

            // 3. SELECTMANY (method syntax)
            // To zapytanie spłaszcza strukturę Order -> Items -> Product
            // Dzięki czemu dostajemy listę wszystkich pozycji zamówień
            // Bardziej naturalna i czytelna do tego jest method syntax, lepiej pokazuje działania krok po kroku naturalne dla spłaszczania struktury
            var flattenedItems = orders
                // Dla każdego zamówienia pobiera jego listę przedmiotów
                .SelectMany(order => order.Items,
                // Tworzy obiekt anonimowy łączący dane z zamówienia i pozycji
                (order, item) => new
                {
                    OrderId = order.Id,
                    CustomerName = order.Customer.Name,
                    ProductName = item.Product.Name,
                    Category = item.Product.Category,
                    item.Quantity,
                    item.TotalPrice
                });

            Console.WriteLine("\n3. Spłaszczone pozycje zamówień (SelectMany, method syntax):");
            foreach (var item in flattenedItems)
            {
                Console.WriteLine($"Zamówienie #{item.OrderId} | {item.CustomerName,-20} | {item.ProductName,-20} | {item.Category,-12} | Ilość: {item.Quantity,-3} | {item.TotalPrice:C}");
            }

            // 4. GROUP BY z agregacją (method syntax)
            // To zapytanie pokazuje top klientów według sumy wartości zamówień
            // Method syntax jest wygodna przy grupowaniu, agregacji i sortowaniu wyniku
            var topCustomers = orders
                // Dzieli zamówienia na grupy według Id klienta
                .GroupBy(o => o.Customer.Id)
                // Dla każdej grupy tworzy nowy obiekt anonimowy z informacją o kliencie, liczbie zamówień i sumie wartości
                .Select(group => new
                {
                    CustomerId = group.Key,
                    CustomerName = group.First().Customer.Name,
                    TotalSpent = group.Sum(o => o.TotalAmount),
                    OrdersCount = group.Count()
                })
                // Sortuje klientów malejąco według sumy wydanej kwoty
                .OrderByDescending(x => x.TotalSpent);

            Console.WriteLine("\n4. Top klienci według wartości zamówień (GroupBy + agregacja, method syntax):");
            foreach (var item in topCustomers)
            {
                Console.WriteLine($"{item.CustomerName,-20} | Zamówienia: {item.OrdersCount,-3} | Suma: {item.TotalSpent:C}");
            }

            // 5. GROUP BY z agregacją per kategoria (query syntax)
            // To zapytanie liczy średnią wartość pozycji zamówienia dla każdej kategorii produktu
            // Query syntax jest tu czytelna, bo łatwo połączyć from, group i select
            var averagePerCategory =
                from order in orders
                from item in order.Items
                // Dzieli przedmioty zamówień na grupy według kategorii produktu
                group item by item.Product.Category into categoryGroup
                // Tworzy nowy obiekt anonimowy dla każdej kategorii, zawierający nazwę kategorii, średnią wartość pozycji i liczbę pozycji
                select new
                {
                    Category = categoryGroup.Key,
                    AverageValue = categoryGroup.Average(i => i.TotalPrice),
                    ItemsCount = categoryGroup.Count()
                };

            Console.WriteLine("\n5. Średnia wartość pozycji per kategoria (GroupBy + agregacja, query syntax):");
            foreach (var item in averagePerCategory)
            {
                Console.WriteLine($"Kategoria: {item.Category,-12} | Średnia: {item.AverageValue:C} | Liczba pozycji: {item.ItemsCount}");
            }

            // 6. GROUPJOIN = left join pattern (method syntax)
            // To zapytanie pokazuje wszystkich klientów, także tych bez zamówień.
            var customersWithOrders = customers
                .GroupJoin(
                // Łączy w parę klientów z zamówieniami, ale zachowuje wszystkich klientów (left join)
                    orders,
                    // porównuje id klienta w zamówieniu z id klienta w liście klientów
                    customer => customer.Id,
                    order => order.Customer.Id,
                    // Tworzy nowy obiekt anonimowy dla każdego klienta, zawierający jego dane oraz listę zamówień (może być pusta)
                    (customer, customerOrders) => new
                    {
                        customer.Name,
                        customer.Email,
                        customer.IsVip,
                        OrdersCount = customerOrders.Count(),
                        TotalSpent = customerOrders.Any() ? customerOrders.Sum(o => o.TotalAmount) : 0m
                    });

            Console.WriteLine("\n6. Wszyscy klienci, także bez zamówień (GroupJoin / left join pattern):");
            foreach (var item in customersWithOrders)
            {
                Console.WriteLine($"{item.Name,-20} | VIP: {item.IsVip,-5} | Zamówienia: {item.OrdersCount,-3} | Suma: {item.TotalSpent:C}");
            }

            // 7. MIXED SYNTAX
            // To zapytanie buduje raport per klient i wyznacza jego ulubioną kategorię
            // na podstawie łącznej kupionej ilości produktów. Najpierw używam query syntax
            // do pobrania danych, a potem method syntax do grupowania i wyboru najlepszej kategorii.
            var customerCategoryRaw =
                from order in orders
                from item in order.Items
                // Tworzy obiekt anonimowy zawierający nazwę klienta, kategorię produktu, ilość i wartość zamówienia
                select new
                {
                    CustomerName = order.Customer.Name,
                    Category = item.Product.Category,
                    Quantity = item.Quantity,
                    OrderAmount = order.TotalAmount
                };

            var favoriteCategoryPerCustomer = customerCategoryRaw
                // Dzieli dane na grupy według nazwy klienta
                .GroupBy(x => x.CustomerName)
                // Dla każdej grupy (klienta) wybiera kategorię
                .Select(group => new
                {
                    CustomerName = group.Key,
                    // Dzieli dane na grupy w obrębie konkretnego klienta
                    FavoriteCategory = group
                    // Dzieli dane na kategorie
                        .GroupBy(x => x.Category)
                        // Sortuje kategorie malejąco według sumy produktów
                        .OrderByDescending(categoryGroup => categoryGroup.Sum(x => x.Quantity))
                        // Pobiera nazwę kategorii z największą sumą produktów
                        .Select(categoryGroup => categoryGroup.Key)
                        // Pobiera pierwszą kategorię z posortowanej listy
                        .FirstOrDefault(),
                    // Oblicza łączną kwotę wydaną przez klienta
                    TotalSpent = orders
                        .Where(o => o.Customer.Name == group.Key)
                        .Sum(o => o.TotalAmount)
                })
                // Sortuje klientów malejąco według łącznej wydanej kwoty
                .OrderByDescending(x => x.TotalSpent);

            Console.WriteLine("\n7. Raport per klient z ulubioną kategorią (mixed syntax):");
            foreach (var item in favoriteCategoryPerCustomer)
            {
                Console.WriteLine($"{item.CustomerName,-20} | Ulubiona kategoria: {item.FavoriteCategory,-12} | Łączna kwota: {item.TotalSpent:C}");
            }
        }
    }
}