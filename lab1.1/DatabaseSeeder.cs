using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace lab1_1_net10
{
    public static class DatabaseSeeder
    {
        // Metoda asynchroniczna do zapełniania bazy danych danymi z SampleData
        public static async Task SeedAsync(OrderFlowContext db)
        {
            // Sprawdzenie czy baza danych już zawiera dane
            if (await db.Products.AnyAsync() ||
                await db.Customers.AnyAsync() ||
                await db.Orders.AnyAsync())
            {
                Console.WriteLine("Baza danych zawiera już dane - pomijam zapełnianie danymi.");
                return;
            }

            // Mapowanie danych z SampleData do obiektów przygotowanych do zapisania w bazie
            var products = SampleData.Products.Select(p => new Product
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            }).ToList();

            // Mapowanie danych z SampleData do obiektów przygotowanych do zapisania w bazie
            var customers = SampleData.Customers.Select(c => new Customer
            {
                Id = c.Id,
                Name = c.Name,
                Email = c.Email,
                IsVip = c.IsVip
            }).ToList();

            // Mapowanie danych z SampleData do obiektów przygotowanych do zapisania w bazie
            var orders = SampleData.Orders.Select(o => new Order
            {
                Id = o.Id,
                CustomerId = o.Customer.Id,
                Customer = null!,
                OrderDate = o.OrderDate,
                Status = o.Status,
                Notes = $"Seed z SampleData dla zamówienia #{o.Id}",
                Items = o.Items.Select(i => new OrderItem
                {
                    ProductId = i.Product.Id,
                    Product = null!,
                    Quantity = i.Quantity,
                    UnitPrice = i.Product.Price
                }).ToList()
            }).ToList();

            // Przekazanie danych do zapisu do pośrednika z bazą danych
            db.Products.AddRange(products);
            db.Customers.AddRange(customers);
            db.Orders.AddRange(orders);
            // Zapisanie zmian w bazie danych
            await db.SaveChangesAsync();

            Console.WriteLine($"Baza wypełniona danymi: produkty={products.Count}, klienci={customers.Count}, zamówienia={orders.Count}.");
        }
    }
}
