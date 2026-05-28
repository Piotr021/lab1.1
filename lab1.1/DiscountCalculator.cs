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

            if (order.TotalAmount > 1000m)
                discountRate += 0.05m;

            // VIP z zamówieniem powyżej 5000 zł dostaje jeszcze dodatkowe 5%.
            if (order.Customer.IsVip && order.TotalAmount > 5000m)
                discountRate += 0.05m;

            return order.TotalAmount * discountRate;
        }

    }
}
