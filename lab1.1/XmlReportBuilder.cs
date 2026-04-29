using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace lab1_1_net10
{
    public class XmlReportBuilder
    {
        // Metoda przyjmująca kolekcję zamówień i generująca raport w formacie XML
        public XDocument BuildReport(IEnumerable<Order> orders)
        {
            // Konwersja kolekcji zamówień na listę, aby móc wielokrotnie ją przetwarzać
            var ordersList = orders.ToList();

            // Tworzenie dokumentu XML z podsumowaniem i szczegółami zamówień według określonego wzoru
            var report = new XDocument(
                new XElement("report",
                    new XAttribute("generated", DateTime.UtcNow.ToString("o")),

                    new XElement("summary",
                        new XAttribute("totalOrders", ordersList.Count),
                        new XAttribute("totalRevenue", ordersList.Sum(o => o.TotalAmount))
                    ),

                    new XElement("byStatus",
                        ordersList
                            .GroupBy(o => o.Status)
                            .Select(group =>
                                new XElement("status",
                                    new XAttribute("name", group.Key),
                                    new XAttribute("count", group.Count()),
                                    new XAttribute("revenue", group.Sum(o => o.TotalAmount))
                                )
                            )
                    ),

                    new XElement("byCustomer",
                        ordersList
                            .GroupBy(o => o.Customer)
                            .Select(group =>
                                new XElement("customer",
                                    new XAttribute("id", group.Key.Id),
                                    new XAttribute("name", group.Key.Name),
                                    new XAttribute("isVip", group.Key.IsVip),

                                    new XElement("orderCount", group.Count()),

                                    new XElement("totalSpent",
                                        group.Sum(o => o.TotalAmount)
                                    ),

                                    new XElement("orders",
                                        group.Select(order =>
                                            new XElement("orderRef",
                                                new XAttribute("id", order.Id),
                                                new XAttribute("total", order.TotalAmount)
                                            )
                                        )
                                    )
                                )
                            )
                    )
                )
            );

            return report;
        }

        // Metoda asynchroniczna zapisująca wygenerowany raport XML do pliku
        // Przyjmuje raport w formie obiektu XDocument oraz ścieżkę do pliku
        public async Task SaveReportAsync(XDocument report, string path)
        {
            // Pobranie katalogu docelowego z podanej ścieżki
            var directory = Path.GetDirectoryName(path);

            // Sprawdzenie, czy katalog istnieje, a jeśli nie, to jego utworzenie
            if (!string.IsNullOrWhiteSpace(directory) &&
                !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            // Zapisanie raportu do pliku w formacie XML
            // Obiekt XDocument jest asynchronicznie przekształcany do postaci String
            // i zapisywany w postaci String do pliku w miejsce które wskazuje scieżka
            await File.WriteAllTextAsync(path, report.ToString());
        }

        // Metoda asynchroniczna odczytująca raport XML z pliku i zwracająca identyfikatory zamówień,
        // których wartość przekracza określony próg
        public async Task<IEnumerable<int>> FindHighValueOrderIdsAsync(
            string reportPath,
            decimal threshold)
        {
            // Odczytanie zawartości pliku XML jako string
            var xmlContent = await File.ReadAllTextAsync(reportPath);

            // Zamienia tekst XML na obiekt XDocument 
            var document = XDocument.Parse(xmlContent);

            // Wyszukanie wszystkich elementów w dokumencie XML, których kwota przekracza podany próg
            var orderIds = document
                .Descendants("orderRef")
                .Where(order =>
                {
                    var total = decimal.Parse(
                        order.Attribute("total")!.Value);

                    return total > threshold;
                })
                // Pobranie identyfikatorów zamówień, które spełniają warunek przekroczenia progu
                .Select(order =>
                    int.Parse(order.Attribute("id")!.Value));

            // Zwrócenie kolekcji identyfikatorów zamówień przekraczających próg
            return orderIds;
        }
    }
}