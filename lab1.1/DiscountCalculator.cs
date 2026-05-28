using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class DiscountCalculator
    {

        public decimal CalculateDiscount(Order order)
        {
            decimal discountRate = 0m;

            if (order.Customer.IsVip)
                discountRate += 0.10m;

            // Zamówienie powyżej 1000 zł dostaje dodatkowe 5%.
            if (order.TotalAmount > 1000m)
                discountRate += 0.05m;

            return order.TotalAmount * discountRate;
        }

    }
}
