using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lab1_1_net10
{
    // tworzenie delegaty ( typ danych przechowujący referencję do odpowiednich metod (reguł) )
    public delegate bool ValidationRule(Order order, out string errorMessage);

    public class OrderValidator
    {
        // readonly przed typem liczbowym lub bool, nie można zmienić wartości
        // readonly przed obiektem, nie można zmienić referencji
        // String obiekt u którego zmiana wartości zmieni referencję 
        private readonly List<ValidationRule> _validationRules = new();
        private readonly List<(Func<Order, bool> Rule, string ErrorMessage)> _funcRules = new();

        public OrderValidator()
        {
            // Reguły oparte na własnym delegacie
            _validationRules.Add(HasItems);
            _validationRules.Add(AllQuantitiesGreaterThanZero);
            _validationRules.Add(DoesNotExceedLimit);

            // Reguły oparte na Func<Order, bool>
            _funcRules.Add((o => o.OrderDate <= DateTime.Now, "Data zamówienia nie może być z przyszłości."));
            _funcRules.Add((o => o.Status != OrderStatus.Cancelled, "Zamówienie nie może mieć statusu Cancelled."));
        }
        // Sprawdzanie wszystkich reguł i zbieranie błędów
        public List<string> ValidateAll(Order order)
        {
            var errors = new List<string>();

            foreach (var rule in _validationRules)
            {
                if (!rule(order, out string errorMessage))
                {
                    errors.Add(errorMessage);
                }
            }

            foreach (var (rule, errorMessage) in _funcRules)
            {
                if (!rule(order))
                {
                    errors.Add(errorMessage);
                }
            }

            return errors;
        }

        // informacja czy wszystko jest poprawne czy są błędy
        public bool IsValid(Order order, out List<string> errors)
        {
            errors = ValidateAll(order);
            return errors.Count == 0;
        }

        // metody spełniające założenia (reguły)

        private bool HasItems(Order order, out string errorMessage)
        {
            if (order.Items == null || !order.Items.Any())
            {
                errorMessage = "Zamówienie musi zawierać co najmniej jedną pozycję.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool AllQuantitiesGreaterThanZero(Order order, out string errorMessage)
        {
            if (order.Items != null && order.Items.Any(i => i.Quantity <= 0))
            {
                errorMessage = "Każda pozycja zamówienia musi mieć ilość większą od 0.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool DoesNotExceedLimit(Order order, out string errorMessage)
        {
            decimal limit = order.Customer?.IsVip == true ? 50000m : 10000m;

            if (order.TotalAmount > limit)
            {
                errorMessage = $"Kwota zamówienia przekracza limit {limit:C}.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}