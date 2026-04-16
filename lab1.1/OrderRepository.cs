using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml.Serialization;

namespace lab1_1_net10
{
    public class OrderRepository
    {
        // Readonly referencja nie może być zmieniona po inicjalizacji
        // JsonSerializerOptions - opcje konfiguracyjne dla serializacji JSON
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            // Formatowanie JSON z wcięciami dla lepszej czytelności
            WriteIndented = true,
            // Użycie camelCase dla nazw właściwości w JSON
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            // Użycie niestandardowego enkodera, który nie ucina znaków Unicode, co pozwala na poprawne wyświetlanie polskich znaków
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        // Metoda do zapisywania kolekcji zamówień do pliku JSON
        // Metoda asynchroniczna, która przyjmuje kolekcję zamówień i ścieżkę do pliku
        // Nie zwraca żadnej wartości
        public async Task SaveToJsonAsync(IEnumerable<Order> orders, string path)
        {
            // Zapewnienie, że katalog docelowy istnieje przed próbą zapisu pliku
            EnsureDirectory(path);

            // Tworzenie strumienia do zapisu danych do pliku
            // await using - zapewnia, że strumień zostanie poprawnie zamknięty i zasoby zwolnione po zakończeniu operacji
            await using var stream = new FileStream(
                path,
                // Otwieranie pliku w trybie tworzenia, co oznacza, że jeśli plik już istnieje, zostanie nadpisany
                FileMode.Create,
                // Ustawienie dostępu do pliku na zapis
                FileAccess.Write,
                // Ustawienie współdzielenia pliku na None czyli inne procesy nie mogą uzyskać dostępu do tego pliku podczas jego zapisu
                FileShare.None,
                // Ustawienie rozmiaru bufora na 4096 bajtów - standardowy rozmiar bufora dla operacji plikowych
                bufferSize: 4096,
                // informuje że strumień ma wspierać operacje asynchroniczne
                useAsync: true);

            // Weź kolekcję zamówień, przekształć ją na listę, a następnie serializuj ją do formatu JSON i zapisz do strumienia(asynchronicznie)
            // Użyj ustawień konfiguracyjnych zdefiniowanych w JsonOptions
            await JsonSerializer.SerializeAsync(stream, orders.ToList(), JsonOptions);
        }

        // Metoda do wczytywania kolekcji zamówień z pliku JSON
        // Metoda asynchroniczna, która przyjmuje ścieżkę do pliku i zwraca listę zamówień
        // Zwraca listę zamówień, która została wczytana z pliku JSON. Jeśli plik nie istnieje, zwraca pustą listę
        public async Task<List<Order>> LoadFromJsonAsync(string path)
        {
            // Sprawdzenie, czy plik istnieje. Jeśli nie, zwróć pustą listę zamówień
            if (!File.Exists(path))
                return new List<Order>();

            // Tworzenie strumienia do odczytu danych z pliku
            // await using - zapewnia, że strumień zostanie poprawnie zamknięty i zasoby zwolnione po zakończeniu operacji
            await using var stream = new FileStream(
                path,
                // Otwieranie pliku w trybie otwierania, co oznacza, że plik musi istnieć, aby operacja się powiodła
                FileMode.Open,
                // Ustawienie dostępu do pliku na odczyt
                FileAccess.Read,
                // Ustawienie współdzielenia pliku na Read, co pozwala innym procesom na odczyt tego pliku, ale nie na jego modyfikację podczas gdy jest otwarty
                FileShare.Read,
                // Ustawienie rozmiaru bufora na 4096 bajtów - standardowy rozmiar bufora dla operacji plikowych
                bufferSize: 4096,
                // informuje że strumień ma wspierać operacje asynchroniczne
                useAsync: true);

            // Odczyt danych z pliku JSON i deserializacja ich do listy zamówień
            var orders = await JsonSerializer.DeserializeAsync<List<Order>>(stream, JsonOptions);
            // Zwróć wczytaną listę zamówień
            // Jeśli deserializacja zwróci null (np: plik jest pusty lub zawiera nieprawidłowy JSON), zwróć pustą listę
            return orders ?? new List<Order>();
        }

        // Metoda do zapisywania kolekcji zamówień do pliku XML
        // Metoda asynchroniczna, która przyjmuje kolekcję zamówień i ścieżkę do pliku
        // Nie zwraca żadnej wartości
        public async Task SaveToXmlAsync(IEnumerable<Order> orders, string path)
        {
            // Zapewnienie, że katalog docelowy istnieje przed próbą zapisu pliku
            EnsureDirectory(path);

            // Tworzenie instancji XmlSerializer dla typu List<Order>, który będzie używany do serializacji danych do formatu XML
            var serializer = new XmlSerializer(typeof(List<Order>));

            // Tworzenie strumienia do zapisu danych do pliku
            // await using - zapewnia, że strumień zostanie poprawnie zamknięty i zasoby zwolnione po zakończeniu operacji
            await using var stream = new FileStream(
                path,
                // Otwieranie pliku w trybie tworzenia, co oznacza, że jeśli plik już istnieje, zostanie nadpisany
                FileMode.Create,
                // Ustawienie dostępu do pliku na zapis
                FileAccess.Write,
                // Ustawienie współdzielenia pliku na None czyli inne procesy nie mogą uzyskać dostępu do tego pliku podczas jego zapisu
                FileShare.None,
                // Ustawienie rozmiaru bufora na 4096 bajtów - standardowy rozmiar bufora dla operacji plikowych
                bufferSize: 4096,
                // informuje że strumień ma wspierać operacje asynchroniczne
                useAsync: true);

            // Weź kolekcję zamówień, przekształć ją na listę, a następnie serializuj ją do formatu XML i zapisz do strumienia
            serializer.Serialize(stream, orders.ToList());
        }

        // Metoda do wczytywania kolekcji zamówień z pliku XML
        public async Task<List<Order>> LoadFromXmlAsync(string path)
        {
            // Sprawdzenie, czy plik istnieje. Jeśli nie, zwróć pustą listę zamówień
            if (!File.Exists(path))
                return new List<Order>();

            // Tworzenie instancji XmlSerializer dla typu List<Order>, który będzie używany do deserializacji danych z formatu XML
            var serializer = new XmlSerializer(typeof(List<Order>));

            // Tworzenie strumienia do odczytu danych z pliku
            // await using - zapewnia, że strumień zostanie poprawnie zamknięty i zasoby zwolnione po zakończeniu operacji
            await using var stream = new FileStream(
                path,
                // Otwieranie pliku w trybie otwierania, co oznacza, że plik musi istnieć, aby operacja się powiodła
                FileMode.Open,
                // Ustawienie dostępu do pliku na odczyt
                FileAccess.Read,
                // Ustawienie współdzielenia pliku na Read, co pozwala innym procesom na odczyt tego pliku, ale nie na jego modyfikację podczas gdy jest otwarty
                FileShare.Read,
                // Ustawienie rozmiaru bufora na 4096 bajtów - standardowy rozmiar bufora dla operacji plikowych
                bufferSize: 4096,
                // informuje że strumień ma wspierać operacje asynchroniczne
                useAsync: true);
            
            // Odczyt danych z pliku XML i deserializacja ich do listy zamówień
            // Ponieważ XmlSerializer nie obsługuje bezpośrednio operacji asynchronicznych, używamy Task.Run
            // aby wykonać deserializację w osobnym wątku, co pozwala na zachowanie asynchroniczności metody
            var result = await Task.Run(() => serializer.Deserialize(stream));
            // Zwróć wczytaną listę zamówień
            // Jeśli deserializacja zwróci null (np: plik jest pusty lub zawiera nieprawidłowy XML), zwróć pustą listę
            return result as List<Order> ?? new List<Order>();
        }

        // Prywatna metoda pomocnicza do zapewnienia, że katalog docelowy istnieje przed próbą zapisu pliku
        private static void EnsureDirectory(string path)
        {
            // Pobranie katalogu z podanej ścieżki.
            string? directory = Path.GetDirectoryName(path);

            // Jeśli katalog jest pusty lub zawiera tylko białe znaki, ustaw go na bieżący katalog
            if (string.IsNullOrWhiteSpace(directory))
                directory = ".";

            // Utworzenie katalogu, jeśli nie istnieje
            Directory.CreateDirectory(directory);
        }
    }
}