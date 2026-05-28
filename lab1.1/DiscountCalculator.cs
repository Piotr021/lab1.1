using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class DiscountCalculator
    {

        public decimal CalculateDiscount(Order order)
        {
            // Klient VIP dostaje 10% rabatu.
            if (order.Customer.IsVip)
                return order.TotalAmount * 0.10m;

            // Zwykły klient przy małym zamówieniu nie dostaje rabatu.
            return 0m;
        }

    }
}
