using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class OrderStatusChangedEventArgs : EventArgs
    {
        public Order Order { get; }
        public OrderStatus OldStatus { get; }
        public OrderStatus NewStatus { get; }
        public DateTime Timestamp { get; }
        
        // konstruktor do utworzenia obiektu zawierającego szczegóły zdarzenia
        public OrderStatusChangedEventArgs(Order order, OrderStatus oldStatus, OrderStatus newStatus, DateTime timestamp)
        {
            Order = order;
            OldStatus = oldStatus;
            NewStatus = newStatus;
            Timestamp = timestamp;
        }
    }
}
