using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    internal class Program
    {
        static void Main(string[] args)
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
        }
    }
}
