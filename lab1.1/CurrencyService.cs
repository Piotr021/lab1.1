using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<decimal?> GetRateAsync(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentException("Kod waluty nie może być pusty.", nameof(currencyCode));

            string code = currencyCode.Trim().ToUpperInvariant();

            if (code == "PLN")
                return 1.0m;

            string url = $"api/exchangerates/rates/A/{code}/?format=json";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.GetAsync(url);
            }
            catch (HttpRequestException ex)
            {
                throw new CurrencyServiceException("Nie udało się połączyć z API NBP.", ex);
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
                throw new CurrencyServiceException($"API NBP zwróciło błąd: {(int)response.StatusCode} {response.ReasonPhrase}");

            try
            {
                await using var stream = await response.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<NbpRateResponse>(stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                decimal? rate = result?.Rates?.FirstOrDefault()?.Mid;
                if (rate == null)
                    throw new CurrencyServiceException("Odpowiedź API NBP nie zawiera kursu średniego.");

                return rate.Value;
            }
            catch (JsonException ex)
            {
                throw new CurrencyServiceException("Nie udało się odczytać odpowiedzi API NBP.", ex);
            }
        }

        public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
        {
            decimal? fromRate = await GetRateAsync(fromCurrency);
            decimal? toRate = await GetRateAsync(toCurrency);

            if (fromRate == null)
                throw new CurrencyServiceException($"Nie znaleziono waluty źródłowej: {fromCurrency}.");

            if (toRate == null)
                throw new CurrencyServiceException($"Nie znaleziono waluty docelowej: {toCurrency}.");

            decimal amountInPln = amount * fromRate.Value;
            return amountInPln / toRate.Value;
        }

        private class NbpRateResponse
        {
            [JsonPropertyName("rates")]
            public NbpRate[]? Rates { get; set; }
        }

        private class NbpRate
        {
            [JsonPropertyName("mid")]
            public decimal Mid { get; set; }
        }
    }
}
