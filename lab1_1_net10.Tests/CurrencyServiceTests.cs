using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using lab1_1_net10;

namespace lab1_1_net10.Tests
{
    public class CurrencyServiceTests
    {
        [Fact]
        public async Task GetRateAsync_PoprawnaWaluta_ZwracaKursSredni()
        {
            var handler = new TestHttpMessageHandler(_ => JsonResponse("""
                { "table":"A", "currency":"dolar amerykański", "code":"USD", "rates":[{ "mid": 4.25 }] }
                """));
            var service = CreateService(handler);

            var rate = await service.GetRateAsync("USD");

            Assert.Equal(4.25m, rate);
        }

        [Fact]
        public async Task GetRateAsync_PLN_ZwracaJedenINieWywolujeApi()
        {
            var handler = new TestHttpMessageHandler(_ => throw new InvalidOperationException("API nie powinno być wywołane dla PLN."));
            var service = CreateService(handler);

            var rate = await service.GetRateAsync("PLN");

            Assert.Equal(1.0m, rate);
            Assert.Equal(0, handler.CallsCount);
        }

        [Fact]
        public async Task GetRateAsync_NieistniejacaWaluta404_ZwracaNull()
        {
            var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
            var service = CreateService(handler);

            var rate = await service.GetRateAsync("XYZ");

            Assert.Null(rate);
        }

        [Fact]
        public async Task GetRateAsync_Blad500_RzucaCurrencyServiceException()
        {
            var handler = new TestHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError));
            var service = CreateService(handler);

            await Assert.ThrowsAsync<CurrencyServiceException>(() => service.GetRateAsync("USD"));
        }

        [Fact]
        public async Task ConvertAsync_DwieRozneWaluty_PrzeliczaPrzezPln()
        {
            var handler = new TestHttpMessageHandler(request =>
            {
                string url = request.RequestUri!.ToString();

                if (url.Contains("/USD/"))
                    return JsonResponse("""{ "rates":[{ "mid": 4.00 }] }""");

                if (url.Contains("/EUR/"))
                    return JsonResponse("""{ "rates":[{ "mid": 5.00 }] }""");

                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            var service = CreateService(handler);

            var result = await service.ConvertAsync(100m, "USD", "EUR");

            Assert.Equal(80m, result);
        }

        [Fact]
        public async Task GetRateAsync_WysylaRequestPodWlasciwyUrl()
        {
            var handler = new TestHttpMessageHandler(_ => JsonResponse("""{ "rates":[{ "mid": 4.25 }] }"""));
            var service = CreateService(handler);

            await service.GetRateAsync("USD");

            Assert.Single(handler.Requests);
            Assert.Equal("https://api.nbp.pl/api/exchangerates/rates/A/USD/?format=json", handler.Requests[0].RequestUri!.ToString());
        }

        private static CurrencyService CreateService(TestHttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.nbp.pl/")
            };

            return new CurrencyService(httpClient);
        }

        private static HttpResponseMessage JsonResponse(string json)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        }
    }
}
