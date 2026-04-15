using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class OrderStatistics
    {
        public int ValidOrders { get; private set; }
        public int InvalidOrders { get; private set; }
        public int CompletedOrders { get; private set; }

        // object? sender - obiekt, który wywołał zdarzenie (może być null)
        // OrderValidationEventArgs e - dane związane z walidacją zamówienia
        public void OnValidationCompleted(object? sender, OrderValidationEventArgs e)
        {
            if (e.IsValid)
                ValidOrders++;
            else
                InvalidOrders++;
        }

        // object? sender - obiekt, który wywołał zdarzenie (może być null)
        // OrderStatusChangedEventArgs e - dane związane ze zmianą statusu zamówienia
        public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e)
        {
            if (e.NewStatus == OrderStatus.Completed)
                CompletedOrders++;
        }

        public void PrintStatistics()
        {
            Console.WriteLine("===== STATYSTYKI =====");
            Console.WriteLine($"Poprawne zamówienia: {ValidOrders}");
            Console.WriteLine($"Niepoprawne zamówienia: {InvalidOrders}");
            Console.WriteLine($"Ukończone zamówienia: {CompletedOrders}");
        }
    }
}
