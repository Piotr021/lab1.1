using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class EmailNotifier
    {
        // object? sender - obiekt, który wywołał zdarzenie (może być null)
        // OrderStatusChangedEventArgs e - dane związane ze zmianą statusu zamówienia
        public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e)
        {
            if (e.Order.Customer != null && !string.IsNullOrWhiteSpace(e.Order.Customer.Email))
            {
                Console.WriteLine(
                    $"[EMAIL] Wysłano email do {e.Order.Customer.Email}: status zamówienia #{e.Order.Id} zmieniono na {e.NewStatus}");
            }
        }
    }
}
