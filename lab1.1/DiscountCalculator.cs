using System;
using System.Collections.Generic;
using System.Text;

namespace lab1_1_net10
{
    public class DiscountCalculator
    {
        private const decimal VipDiscountRate = 0.10m;
        private const decimal HighValueDiscountRate = 0.05m;
        private const decimal VipHighValueDiscountRate = 0.05m;

        private const decimal HighValueThreshold = 1000m;
        private const decimal VipHighValueThreshold = 5000m;

        private const decimal MaxDiscountRate = 0.25m;

        public decimal CalculateDiscount(Order order)
        {
            // Normalne użycie kalkulatora.
            // Nie dodajemy tutaj żadnych sztucznych promocji.
            return CalculateDiscount(order, 0m);
        }

        public decimal CalculateDiscount(Order order, decimal extraDiscountRate)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // Najpierw liczymy procent rabatu.
            decimal discountRate = CalculateDiscountRate(order);

            // Ten dodatkowy rabat pomaga sprawdzić limit 25%.
            // Może też przydać się kiedyś przy promocjach sezonowych.
            discountRate += extraDiscountRate;

            // Na końcu pilnujemy, żeby rabat nie był większy niż 25%.
            discountRate = LimitDiscountRate(discountRate);

            // Zadanie wymaga, żeby zwracać kwotę rabatu w PLN, a nie procent.
            return order.TotalAmount * discountRate;
        }

        private decimal CalculateDiscountRate(Order order)
        {
            decimal discountRate = 0m;

            if (IsVipCustomer(order))
                discountRate += VipDiscountRate;

            if (IsHighValueOrder(order))
                discountRate += HighValueDiscountRate;

            if (IsVipHighValueOrder(order))
                discountRate += VipHighValueDiscountRate;

            return discountRate;
        }


        private bool IsVipCustomer(Order order)
        {
            return order.Customer != null && order.Customer.IsVip;
        }

        private bool IsHighValueOrder(Order order)
        {
            return order.TotalAmount > HighValueThreshold;
        }

        private bool IsVipHighValueOrder(Order order)
        {
            return IsVipCustomer(order) && order.TotalAmount > VipHighValueThreshold;
        }

        private decimal LimitDiscountRate(decimal discountRate)
        {
            if (discountRate > MaxDiscountRate)
                return MaxDiscountRate;

            return discountRate;
        }
    }
}