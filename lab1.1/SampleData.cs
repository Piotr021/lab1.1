using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    public static class SampleData
    {
        public static List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Laptop", Category = "Electronics", Price = 3500m },
            new Product { Id = 2, Name = "Smartphone", Category = "Electronics", Price = 2200m },
            new Product { Id = 3, Name = "Desk Chair", Category = "Furniture", Price = 800m },
            new Product { Id = 4, Name = "Coffee Maker", Category = "Home Appliances", Price = 300m },
            new Product { Id = 5, Name = "Headphones", Category = "Electronics", Price = 450m }
        };

        public static List<Customer> Customers = new List<Customer>
        {
            new Customer { Id = 1, Name = "Jan Kowalski", Email = "jan@example.com", IsVip = false },
            new Customer { Id = 2, Name = "Anna Nowak", Email = "anna@example.com", IsVip = true },
            new Customer { Id = 3, Name = "Piotr Wiśniewski", Email = "piotr@example.com", IsVip = false },
            new Customer { Id = 4, Name = "Katarzyna Zielińska", Email = "kasia@example.com", IsVip = true }
        };

        public static List<Order> Orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                Customer = Customers[0],
                OrderDate = DateTime.Now.AddDays(-10),
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[0], Quantity = 1 },
                    new OrderItem { Product = Products[4], Quantity = 2 }
                }
            },
            new Order
            {
                Id = 2,
                Customer = Customers[1],
                OrderDate = DateTime.Now.AddDays(-8),
                Status = OrderStatus.Processing,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[1], Quantity = 1 },
                    new OrderItem { Product = Products[3], Quantity = 1 }
                }
            },
            new Order
            {
                Id = 3,
                Customer = Customers[2],
                OrderDate = DateTime.Now.AddDays(-5),
                Status = OrderStatus.Cancelled,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[2], Quantity = 0 }
                }
            },
            new Order
            {
                Id = 4,
                Customer = Customers[3],
                OrderDate = DateTime.Now.AddDays(-4),
                Status = OrderStatus.Validated,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[0], Quantity = 1 },
                    new OrderItem { Product = Products[1], Quantity = 2 }
                }
            },
            new Order
            {
                Id = 5,
                Customer = Customers[1],
                OrderDate = DateTime.Now.AddDays(-2),
                Status = OrderStatus.New,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[3], Quantity = 3 }
                }
            },
            new Order
            {
                Id = 6,
                Customer = Customers[0],
                OrderDate = DateTime.Now.AddDays(-1),
                Status = OrderStatus.Completed,
                Items = new List<OrderItem>
                {
                    new OrderItem { Product = Products[4], Quantity = 1 },
                    new OrderItem { Product = Products[2], Quantity = 2 }
                }
            }
        };
    }
}
