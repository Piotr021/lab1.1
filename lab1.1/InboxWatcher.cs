using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace lab1_1_net10
{
    // InboxWatcher implementuje interfejs IDisposable który pozwala na zwolnienie zasobów
    public class InboxWatcher : IDisposable
    {
        // readonly - referencja do obiektu nie może być zmieniona po przypisaniu
        // FileSystemWatcher - klasa z przestrzeni nazw System.IO, która umożliwia monitorowanie zmian
        // w systemie plików, takich jak tworzenie, modyfikowanie czy usuwanie plików
        private readonly FileSystemWatcher _watcher;
        // OrderPipeline - klasa odpowiedzialna za przetwarzanie zamówień
        private readonly OrderPipeline _pipeline;
        // SemaphoreSlim - klasa z przestrzeni nazw System.Threading,
        // która umożliwia ograniczenie liczby wątków
        private readonly SemaphoreSlim _semaphore = new(2);

        // Ścieżki do katalogów: inbox, processed i failed
        private readonly string _inboxPath;
        private readonly string _processedPath;
        private readonly string _failedPath;

        // Konstruktor przyjmuje ścieżkę do katalogu inbox oraz instancję klasy OrderPipeline
        public InboxWatcher(string inboxPath, OrderPipeline pipeline)
        {
            // Instrukcja jak mają być przetwarzane zamówienia
            _pipeline = pipeline;

            // Ścieżki do folderów
            _inboxPath = inboxPath;
            _processedPath = Path.Combine(inboxPath, "processed");
            _failedPath = Path.Combine(inboxPath, "failed");

            // Utworzenie katalogów, jeśli nie istnieją
            Directory.CreateDirectory(_inboxPath);
            Directory.CreateDirectory(_processedPath);
            Directory.CreateDirectory(_failedPath);

            // Utworzenie FileSystemWatcher, który będzie monitorował katalog inbox i reagował
            // na tworzenie nowych plików z rozszerzeniem .json
            _watcher = new FileSystemWatcher(_inboxPath, "*.json");

            // Gdy w folderze pojawi się nowy plik, zostanie wywołana metoda OnCreated
            _watcher.Created += OnCreated;
            // Uruchomienie monitorowania folderu
            _watcher.EnableRaisingEvents = true;

            Console.WriteLine($"Watcher aktywny: {_inboxPath}");
        }

        // Metoda wywoływana, gdy zostanie utworzony nowy plik w folderze inbox
        // Uruchamia asynchroniczne przetwarzanie pliku
        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            _ = Task.Run(() => ProcessFileAsync(e.FullPath));
        }

        // Asynchroniczna metoda przetwarzająca plik z zamówieniami
        private async Task ProcessFileAsync(string path)
        {
            // Ograniczenie liczby jednoczesnych przetwarzań plików do liczby określonej przez semafor
            await _semaphore.WaitAsync();

            try
            {
                Console.WriteLine($"Wykryto plik: {Path.GetFileName(path)}");

                // Opóźnienie losowe między 200 a 500 ms
                await Task.Delay(Random.Shared.Next(200, 500));

                // orders zmienna przechowująca listę zamówień
                List<Order>? orders = null;

                for (int i = 0; i < 5; i++)
                {
                    try
                    {
                        // Odczytywanie zawartości pliku JSON
                        var json = await File.ReadAllTextAsync(path);

                        // Deserializacja JSON do listy obiektów Order
                        orders = JsonSerializer.Deserialize<List<Order>>(json,
                            new JsonSerializerOptions
                            {
                                // Ignorowanie wielkości liter w nazwach właściwości JSON
                                PropertyNameCaseInsensitive = true
                            });

                        break;
                    }
                    catch (IOException)
                    {
                        // Jeśli plik jest nadal używany czekamy 300 ms i próbujemy ponownie
                        await Task.Delay(300);
                    }
                }

                // Jeśli nie udało się odczytać pliku albo lista zamówień jest pusta, rzucamy wyjątek
                if (orders == null || orders.Count == 0)
                    throw new Exception("Brak zamówień w pliku.");

                Console.WriteLine($"Zaimportowano {orders.Count} zamówień.");

                // Przetwarzanie zamówień
                foreach (var order in orders)
                {
                    _pipeline.ProcessOrder(order);
                }

                // Tworzenie ścieżki docelowej w folderze processed
                var destination = Path.Combine(
                    _processedPath,
                    Path.GetFileName(path));

                // Jeśli plik o tej samej nazwie już istnieje w folderze processed, usuwamy go
                if (File.Exists(destination))
                    File.Delete(destination);

                // Przenoszenie pliku do folderu processed
                File.Move(path, destination);

                Console.WriteLine($"Plik przeniesiony do processed/");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd importu: {ex.Message}");

                try
                {
                    // Tworzenie ścieżki docelowej w folderze failed
                    var failedFile = Path.Combine(
                        _failedPath,
                        Path.GetFileName(path));

                    // Jeśli plik o tej samej nazwie już istnieje w folderze failed, usuwamy go
                    if (File.Exists(failedFile))
                        File.Delete(failedFile);

                    // Przenoszenie pliku do folderu failed
                    File.Move(path, failedFile);

                    //
                    var errorFile = failedFile + ".error.txt";

                    // Zapis błędu do pliku tekstowego
                    await File.WriteAllTextAsync(errorFile, ex.ToString());

                    Console.WriteLine($"Plik przeniesiony do failed/");
                }
                catch (Exception moveEx)
                {
                    Console.WriteLine($"Błąd przenoszenia pliku: {moveEx.Message}");
                }
            }
            finally
            {
                // Zwolnienie semafora, aby inne pliki mogły być przetwarzane
                _semaphore.Release();
            }
        }

        // Metoda czyszcząca zasoby
        public void Dispose()
        {
            // Wyłączenie monitorowania folderu
            _watcher.EnableRaisingEvents = false;

            // Zwalnienie zasobów związanych z FileSystemWatcher i SemaphoreSlim
            _watcher.Dispose();
            _semaphore.Dispose();

            Console.WriteLine("Watcher zatrzymany.");
        }
    }
}