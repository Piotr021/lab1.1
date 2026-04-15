using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;


namespace lab1_1_net10
{
    public class OrderPipeline
    {
        // Zdarzenie informujące o zmianie statusu zamówienia
        public event EventHandler<OrderStatusChangedEventArgs>? StatusChanged;
        // Zdarzenie informujące o zakończeniu walidacji zamówienia
        public event EventHandler<OrderValidationEventArgs>? ValidationCompleted;

        public void ProcessOrder(Order order)
        {
            // Wyjątek, jeśli zamówienie jest null(nie istnieje)
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            // Lista błędów walidacji zamówienia
            var errors = ValidateOrder(order);
            // True jeśli zamówienie nie zawiera błędów
            bool isValid = errors.Count == 0;

            // Wywołanie metody odpalającej zdarzenie informujące o zakończeniu walidacji
            // OrderValidationEventArgs(dane zdarzenia)
            // this w implementacji metody oznacza bieżący obiekt OrderPipeline, na którym wywołano ProcessOrder
            OnValidationCompleted(new OrderValidationEventArgs(order, isValid, errors));

            if (!isValid)
            {
                Console.WriteLine($"Przetwarzanie zamówienia #{order.Id} przerwane - walidacja nie powiodła się.");
                Console.WriteLine();
                return;
            }

            // Zmiana statusu zamówienia
            ChangeStatus(order, OrderStatus.Validated);
            // Symulacja pracy
            SimulateWork();

            ChangeStatus(order, OrderStatus.Processing);
            SimulateWork();

            ChangeStatus(order, OrderStatus.Completed);
            SimulateWork();

            Console.WriteLine($"Zamówienie #{order.Id} zostało poprawnie przetworzone.");
            Console.WriteLine();
        }

        // Metoda walidująca zamówienie i zwracająca listę błędów
        private List<string> ValidateOrder(Order order)
        {
            // Lista błędów
            var errors = new List<string>();

            if (order.Customer == null)
            {
                errors.Add("Brak klienta przypisanego do zamówienia.");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(order.Customer.Name))
                    errors.Add("Klient nie ma podanego imienia/nazwy.");

                if (string.IsNullOrWhiteSpace(order.Customer.Email))
                    errors.Add("Klient nie ma podanego adresu email.");
            }

            if (order.Items == null || !order.Items.Any())
            {
                errors.Add("Zamówienie nie zawiera żadnych produktów.");
            }
            else
            {
                if (order.Items.Any(i => i.Product == null))
                    errors.Add("Istnieją pozycje zamówienia bez przypisanego produktu.");

                if (order.Items.Any(i => i.Quantity <= 0))
                    errors.Add("Istnieją pozycje zamówienia z niepoprawną ilością.");

                if (order.Items.Any(i => i.Product != null && i.Product.Price < 0))
                    errors.Add("Istnieją pozycje zamówienia z ujemną ceną.");
            }

            if (order.TotalAmount <= 0)
                errors.Add("Łączna wartość zamówienia musi być większa od 0.");

            return errors;
        }

        private void ChangeStatus(Order order, OrderStatus newStatus)
        {
            var oldStatus = order.Status;
            order.Status = newStatus;
            // Wywołanie metody odpalającej zdarzenie informujące o zmianie statusu zamówienia, przekazując odpowiednie dane w OrderStatusChangedEventArgs
            OnStatusChanged(new OrderStatusChangedEventArgs(
                order,
                oldStatus,
                newStatus,
                DateTime.Now));
        }
        // virtual - pozwala klasom dziedziczącym nadpisać tę metodę, łatwiejsze do rozwijania w przyszłości
        protected virtual void OnStatusChanged(OrderStatusChangedEventArgs e)
        {
            // ?. - operator warunkowego dostępu, który sprawdza, czy StatusChanged jest różne od null przed próbą wywołania zdarzenia.
            // Zapobiega to potencjalnemu rzuceniu wyjątku NullReferenceException, jeśli nikt nie subskrybuje zdarzenia.
            // invoke - wywołuje zdarzenie, przekazując bieżący obiekt jako nadawcę (this) oraz dane zdarzenia (e).
            StatusChanged?.Invoke(this, e);
        }

        protected virtual void OnValidationCompleted(OrderValidationEventArgs e)
        {
            ValidationCompleted?.Invoke(this, e);
        }

        private void SimulateWork()
        {
            // Opóźnienie o 500 ms, symulujące czas potrzebny na wykonanie operacji
            Thread.Sleep(500);
        }
    }
}
