using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class DiscountCalculator
    {

        public decimal CalculateDiscount(Order order)
        {
            return CalculateDiscount(order, 0m);
        }

        public decimal CalculateDiscount(Order order, decimal extraDiscountRate)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            decimal discountRate = 0m;

            if (order.Customer.IsVip)
                discountRate += 0.10m;

            if (order.TotalAmount > 1000m)
                discountRate += 0.05m;

            if (order.Customer.IsVip && order.TotalAmount > 5000m)
                discountRate += 0.05m;

            // Dodatkowy rabat pozwala łatwo sprawdzić w teście,
            // czy limit 25% naprawdę działa.
            discountRate += extraDiscountRate;

            if (discountRate > 0.25m)
                discountRate = 0.25m;

            return order.TotalAmount * discountRate;
        }

    }
}
