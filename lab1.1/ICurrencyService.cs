using System.Threading.Tasks;

namespace lab1_1_net10
{
    public interface ICurrencyService
    {
        Task<decimal?> GetRateAsync(string currencyCode);
        Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
    }
}
