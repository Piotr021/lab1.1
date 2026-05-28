using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using lab1_1_net10;

namespace lab1_1_net10.Tests
{
    public class DiscountCalculatorTests
    {
        [Fact]
        public void StandardowyKlient_MaleZamowienie_ZwracaZeroRabatu()
        {
            // Arrange - przygotowanie danych do testu
            var order = CreateOrder(isVip: false, price: 500m);

            // Tworzenie instancji kalkulatora 
            var calculator = new DiscountCalculator();

            // Act - wykonanie testowanej metody
            var discount = calculator.CalculateDiscount(order);

            // Assert - sprawdzenie wyniku
            Assert.Equal(0m, discount);
        }

        [Fact]
        public void KlientVip_MaleZamowienie_ZwracaDziesiecProcentRabatu()
        {
            // Arrange
            var order = CreateOrder(isVip: true, price: 500m);
            var calculator = new DiscountCalculator();

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            // 10% z 500 zł = 50 zł
            Assert.Equal(50m, discount);
        }

        [Fact]
        public void StandardowyKlient_ZamowieniePowyzej1000_ZwracaPiecProcentRabatu()
        {
            // Arrange
            var order = CreateOrder(isVip: false, price: 1200m);
            var calculator = new DiscountCalculator();

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            // 5% z 1200 zł = 60 zł
            Assert.Equal(60m, discount);
        }

        [Fact]
        public void KlientVip_ZamowieniePowyzej1000_ZwracaPietnascieProcentRabatu()
        {
            // Arrange
            var order = CreateOrder(isVip: true, price: 2000m);
            var calculator = new DiscountCalculator();

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            // 10% za VIP + 5% za kwotę powyżej 1000 zł = 15%
            // 15% z 2000 zł = 300 zł
            Assert.Equal(300m, discount);
        }

        [Fact]
        public void KlientVip_ZamowieniePowyzej5000_ZwracaDwadziesciaProcentRabatu()
        {
            // Arrange
            var order = CreateOrder(isVip: true, price: 6000m);
            var calculator = new DiscountCalculator();

            // Act
            var discount = calculator.CalculateDiscount(order);

            // Assert
            // 10% za VIP + 5% za kwotę powyżej 1000 zł + 5% za VIP powyżej 5000 zł = 20%
            // 20% z 6000 zł = 1200 zł
            Assert.Equal(1200m, discount);
        }

        private static Order CreateOrder(bool isVip, decimal price)
        {
            return new Order
            {
                Id = 1,
                Customer = new Customer
                {
                    Id = 1,
                    Name = "Testowy klient",
                    Email = "test@example.com",
                    IsVip = isVip
                },
                OrderDate = DateTime.Now,
                Status = OrderStatus.New,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Product = new Product
                        {
                            Id = 1,
                            Name = "Produkt testowy",
                            Category = "Test",
                            Price = price
                        },
                        Quantity = 1
                    }
                }
            };
        }

    }
}
