using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using lab1_1_net10;

namespace lab1_1_net10.Tests
{
    public class OrderCurrencyConverterTests
    {
        [Fact]
        public async Task ConvertOrderTotalAsync_PrzeliczaTotalAmountNaWaluteDocelowa()
        {
            var order = CreateOrder(100m, 2); // TotalAmount = 200
            var currencyServiceMock = new Mock<ICurrencyService>();
            currencyServiceMock
                .Setup(s => s.ConvertAsync(200m, "PLN", "USD"))
                .ReturnsAsync(50m);

            var converter = new OrderCurrencyConverter(currencyServiceMock.Object);

            var result = await converter.ConvertOrderTotalAsync(order, "USD");

            Assert.Equal(50m, result);
            currencyServiceMock.Verify(s => s.ConvertAsync(200m, "PLN", "USD"), Times.Once);
        }

        [Fact]
        public async Task ConvertOrderTotalAsync_NullOrder_RzucaArgumentNullExceptionINieWolaSerwisu()
        {
            var currencyServiceMock = new Mock<ICurrencyService>();
            var converter = new OrderCurrencyConverter(currencyServiceMock.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(() => converter.ConvertOrderTotalAsync(null!, "EUR"));
            currencyServiceMock.Verify(s => s.ConvertAsync(It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        private static Order CreateOrder(decimal price, int quantity)
        {
            return new Order
            {
                Customer = new Customer { Name = "Test", Email = "test@example.com" },
                OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product { Name = "Test", Price = price },
                        Quantity = quantity
                    }
                }
            };
        }
    }
}
