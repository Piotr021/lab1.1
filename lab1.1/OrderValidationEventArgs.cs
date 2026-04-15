using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class OrderValidationEventArgs : EventArgs
    {
        public Order Order { get; }
        public bool IsValid { get; }
        public List<string> Errors { get; }

        // konstruktor do utworzenia obiektu zawierającego szczegóły walidacji zamówienia
        public OrderValidationEventArgs(Order order, bool isValid, List<string> errors)
        {
            Order = order;
            IsValid = isValid;
            Errors = errors;
        }
    }
}
