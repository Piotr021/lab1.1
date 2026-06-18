using Xunit;
using lab1_1_net10;
using System;
using System.Collections.Generic;
using System.Linq;

namespace lab1_1_net10.Tests
{
    public class OrderValidatorTests
    {
        // Tworzenie poprawnego zamówienia do testów
        private static Order CreateValidOrder(OrderStatus status = OrderStatus.New)
        {
            return new Order
            {
                Customer = new Customer { IsVip = false },
                OrderDate = DateTime.Now.AddDays(-1),
                Status = status,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Quantity = 1,
                        Product = new Product { Price = 100m }
                    }
                }
            };
        }

        // Sprawdzanie walidacji zamówienia bez pozycji
        [Fact]
        public void ValidateAll_BrakujePozycji_ZwracaBlad()
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia
            var order = CreateValidOrder();
            // Usuwanie pozycji z zamówienia
            order.Items = new List<OrderItem>();

            // Act - Wykonaj
            // Walidacja zamówienia
            var errors = validator.ValidateAll(order);

            // Assert - Sprawdź
            // Sprawdzenie czy zwrócony błąd zawiera informację o braku pozycji
            Assert.Contains(errors, e => e.Contains("co najmniej jedną pozycję"));
        }

        // Sprawdzanie walidacji zamówienia z ilością pozycji równą zero
        [Fact]
        public void ValidateAll_IloscPozycjiJestZero_ZwracaBlad()
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia
            var order = CreateValidOrder();
            // Ustawienie ilości pierwszej pozycji na zero
            order.Items[0].Quantity = 0;

            // Act - Wykonaj
            // Walidacja zamówienia
            var errors = validator.ValidateAll(order);

            // Assert - Sprawdź
            // Sprawdzenie czy zwrócony błąd zawiera informację o ilości większej od 0
            Assert.Contains(errors, e => e.Contains("ilość większą od 0"));
        }

        // Sprawdzanie walidacji zamówienia, którego wartość przekracza limit
        [Fact]
        public void ValidateAll_KwotaPrzekraczaLimit_ZwracaBlad()
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia
            var order = CreateValidOrder();
            // Ustawienie ceny produktu na wartość przekraczającą limit
            order.Items[0].Product.Price = 10001m;

            // Act - Wykonaj
            // Walidacja zamówienia
            var errors = validator.ValidateAll(order);

            // Assert - Sprawdź
            // Sprawdzenie czy zwrócony błąd zawiera informację o przekroczeniu limitu
            Assert.Contains(errors, e => e.Contains("przekracza limit"));
        }

        // Sprawdzanie walidacji zamówienia z datą w przyszłości
        [Fact]
        public void ValidateAll_DataZPrzyszlosci_ZwracaBlad()
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia
            var order = CreateValidOrder();
            // Ustawienie daty zamówienia na przyszłość
            order.OrderDate = DateTime.Now.AddDays(1);

            // Act - Wykonaj
            // Walidacja zamówienia
            var errors = validator.ValidateAll(order);

            // Assert - Sprawdź
            // Sprawdzenie czy zwrócony błąd zawiera informację o dacie w przyszłości
            Assert.Contains(errors, e => e.Contains("przyszłości"));
        }

        // test parametryzowany - test uruchamia się wiele razy z różnymi danymi wejściowymi
        [Theory]
        [InlineData(OrderStatus.New, true)]
        [InlineData(OrderStatus.Validated, true)]
        [InlineData(OrderStatus.Processing, true)]
        [InlineData(OrderStatus.Completed, true)]
        [InlineData(OrderStatus.Cancelled, false)]

        // Sprawdzanie walidacji zamówienia dla różnych statusów
        public void IsValid_RozneStatusy_ZwracaOczekiwanyWynik(OrderStatus status, bool expected)
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia z określonym statusem
            var order = CreateValidOrder(status);

            // Act - Wykonaj
            // Walidacja zamówienia i pobranie błędów
            var result = validator.IsValid(order, out var errors);

            // Assert - Sprawdź
            // Sprawdzenie czy wynik walidacji jest zgodny z oczekiwanym ( takim jak w InlineData)
            Assert.Equal(expected, result);
            Assert.Equal(expected, errors.Count == 0);
        }

        // Sprawdzanie walidacji zamówienia, które narusza kilka reguł jednocześnie
        [Fact]
        public void ValidateAll_KilkaRegulNaraz_ZwracaKilkaBledow()
        {
            // Arrange - Przygotuj
            // Tworzenie instancji walidatora
            var validator = new OrderValidator();
            // Tworzenie zamówienia z kilkoma błędami
            // Ustawienie statusu na Cancelled
            var order = CreateValidOrder(OrderStatus.Cancelled);
            // Ustawienie daty zamówienia na przyszłość
            order.OrderDate = DateTime.Now.AddDays(1);
            // Ustawienie ilości pierwszej pozycji na zero
            order.Items[0].Quantity = 0;

            // Act - Wykonaj
            // Walidacja zamówienia
            var errors = validator.ValidateAll(order);

            // Assert - Sprawdź
            // Sprawdzenie czy zwrócony błąd zawiera informacje o wszystkich naruszonych regułach
            // Sprawdzenie czy liczba błędów się zgadza z oczekiwaną
            Assert.True(errors.Count >= 3);
            // Sprawdzanie konkretnych kounikatów błędów
            Assert.Contains(errors, e => e.Contains("ilość większą od 0"));
            Assert.Contains(errors, e => e.Contains("przyszłości"));
            Assert.Contains(errors, e => e.Contains("Cancelled"));
        }

        // Sprawdzanie filtrowania zamówień po statusie
        [Fact]
        public void FilterOrders_StatusNew_ZwracaTylkoNoweZamowienia()
        {
            // Arrange - Przygotuj
            // Tworzenie zestawu zamówień o różnych statusach
            var orders = new[]
            {
                CreateValidOrder(OrderStatus.New),
                CreateValidOrder(OrderStatus.Completed),
                CreateValidOrder(OrderStatus.New)
            };
            // Tworzenie instancji procesora zamówień do wykonywania na nich operacji
            var processor = new OrderProcessor(orders);

            // Act - Wykonaj
            // Filtrowanie zamówień o statusie New
            var result = processor.FilterOrders(o => o.Status == OrderStatus.New).ToList();

            // Assert - Sprawdź
            // Sprawdzenie czy liczba wyników jest zgodna z oczekiwaniami
            Assert.Equal(2, result.Count);
            // Sprawdzenie czy wszystkie zwrócone zamówienia mają status New
            Assert.All(result, o => Assert.Equal(OrderStatus.New, o.Status));
        }

        // Sprawdzanie agregacji zamówień - sumowanie wartości zamówień
        [Fact]
        public void AggregateOrders_SumaWartosci_ZwracaSumeZamowien()
        {
            // Arrange - Przygotuj
            // Tworzenie zamówienia
            var order1 = CreateValidOrder();
            // Ustawienie ceny produktu na 100
            order1.Items[0].Product.Price = 100m;

            // Tworzenie drugiego zamówienia
            var order2 = CreateValidOrder();
            // Ustawienie ceny produktu na 250
            order2.Items[0].Product.Price = 250m;

            // Tworzenie instancji procesora zamówień do wykonywania na nich operacji
            var processor = new OrderProcessor(new[] { order1, order2 });

            // Act - Wykonaj
            // Agregacja zamówień - sumowanie wartości zamówień
            var total = processor.AggregateOrders(orders => orders.Sum(o => o.TotalAmount));

            // Assert - Sprawdź
            // Sprawdzenie czy suma wartości zamówień jest zgodna z oczekiwaniami
            Assert.Equal(350m, total);
        }
    }
}