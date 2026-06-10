using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    public class OrderCurrencyConverter
    {
        private readonly ICurrencyService _currencyService;

        public OrderCurrencyConverter(ICurrencyService currencyService)
        {
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        }

        public Task<decimal> ConvertOrderTotalAsync(Order order, string targetCurrency)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            return _currencyService.ConvertAsync(order.TotalAmount, "PLN", targetCurrency);
        }
    }
}
