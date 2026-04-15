using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace lab1_1_net10
{
    public class OrderProcessor
    {
        private readonly List<Order> _orders;

        public OrderProcessor(IEnumerable<Order> orders)
        {
            _orders = orders.ToList();
        }

        // Predicate<Order> — filtrowanie zamówień
        // IEnumerable pozwala operować na dowolnej kolekcji
        // Predicate<Typ danych> delegat, który przyjmuje dowolny typ danych i zwraca bool 
        // Mechanizm filtracji, zwraca zamówienia jeśli wynikiem będzie true,logika filtrowania do zaimplementowania
        public IEnumerable<Order> FilterOrders(Predicate<Order> predicate)
        {
            return _orders.Where(order => predicate(order));
        }

        // Action<Order> — wykonanie akcji na zamówieniach
        // IEnumerable pozwala operować na dowolnej kolekcji
        // Action<Typ danych> delegat, który przyjmuje dowolny typ danych i wykonuje na nim jakąś akcję,do zaimplementowania
        public void ProcessOrders(IEnumerable<Order> orders, Action<Order> action)
        {
            foreach (var order in orders)
            {
                action(order);
            }
        }

        // Func<Order, T> — projekcja na dowolny typ
        // IEnumerable pozwala operować na dowolnej kolekcji
        // ProjectOrders przyjmuje zamówienie i zmienia je w inny typ danych, do zaimplementowania
        public IEnumerable<T> ProjectOrders<T>(Func<Order, T> selector)
        {
            return _orders.Select(selector);
        }

        // Agregacja — Func<IEnumerable<Order>, decimal>
        // IEnumerable pozwala operować na dowolnej kolekcji
        // AggregateOrders metoda do agregacji danych, wyliczania sum, średnich, największej wartości itp., do zaimplementowania
        public decimal AggregateOrders(Func<IEnumerable<Order>, decimal> aggregator)
        {
            return aggregator(_orders);
        }

        // Łańcuch: filtruj -> sortuj -> weź top N -> wypisz
        // złożony mechanizm do wykonania kilku operacji na raz, do zaimplementowania
        public void FilterSortTakeAndPrint(
            Predicate<Order> predicate,
            Func<Order, object> orderBy,
            int top,
            Action<Order> action)
        {
            var result = _orders
                .Where(order => predicate(order))
                .OrderByDescending(orderBy)
                .Take(top);

            foreach (var order in result)
            {
                action(order);
            }
        }
    }
}