using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class ConsoleLogger
    {
        // object? sender - obiekt, który wywołał zdarzenie (może być null)
        // OrderValidationEventArgs e - dane związane z walidacją zamówienia
        public void OnValidationCompleted(object? sender, OrderValidationEventArgs e)
        {
            // e.IsValid - informacja, czy zamówienie jest poprawne dla konkretnego zamówienia
            if (e.IsValid)
            {
                Console.WriteLine($"[LOG] Walidacja zamówienia #{e.Order.Id}: poprawna");
            }
            else
            {
                Console.WriteLine($"[LOG] Walidacja zamówienia #{e.Order.Id}: błędna");
                foreach (var error in e.Errors)
                {
                    Console.WriteLine($"      - {error}");
                }
            }
        }

        public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e)
        {
            Console.WriteLine(
                $"[LOG] {e.Timestamp:HH:mm:ss} | Zamówienie #{e.Order.Id} | {e.OldStatus} -> {e.NewStatus}");
        }
    }
}
