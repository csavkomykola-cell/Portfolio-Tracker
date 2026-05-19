using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Portfolio_Tracker.Services
{
    public class PriceService : IDisposable
    {
        private readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        private CancellationTokenSource _cts;
        private Task _refreshLoopTask;

        public int RefreshIntervalSeconds { get; set; } = 60;

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (_isLoading == value) return;
                _isLoading = value;
                LoadingChanged?.Invoke(_isLoading);
            }
        }

        // Подія, що викликається в UI-потоці при зміні IsLoading
        public event Action<bool> LoadingChanged;

        // Подія, що викликається в UI-потоці з словником символ->ціна (символ = верхній регістр)
        public event Action<Dictionary<string, decimal>> PricesUpdated;

        // Подія для помилок (UI може логувати або показувати неблокуюче повідомлення)
        public event Action<Exception> PricesUpdateFailed;

        private static readonly Dictionary<string, string> _symbolToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "BTC", "bitcoin" },
            { "ETH", "ethereum" },
            { "LTC", "litecoin" },
            { "DOGE", "dogecoin" },
            { "ADA", "cardano" },
            { "BNB", "binancecoin" },
            { "SOL", "solana" },
            { "XRP", "ripple" },
            { "DOT", "polkadot" },
            { "BCH", "bitcoin-cash" },
            { "LINK", "chainlink" },
            { "USDT", "tether" },
            { "USDC", "usd-coin" },
            { "WBTC", "wrapped-bitcoin" }
        };

        public PriceService() { }

        public async Task RefreshPricesAsync(IEnumerable<string> symbols)
        {
            var list = symbols?.Where(s => !string.IsNullOrWhiteSpace(s))
                               .Select(s => s.Trim().ToUpperInvariant())
                               .Distinct()
                               .ToList() ?? new List<string>();

            if (list.Count == 0) return;

            // split crypto vs others
            var idMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var s in list)
                if (_symbolToId.TryGetValue(s, out var id))
                    idMap[s] = id;

            var nonCrypto = list.Except(idMap.Keys, StringComparer.OrdinalIgnoreCase).ToList();

            IsLoading = true;
            try
            {
                var prices = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);

                // 1) CoinGecko для відомих ID криптовалют
                if (idMap.Count > 0)
                {
                    var ids = string.Join(",", idMap.Values.Distinct());
                    var vsCurrencies = "usd";
                    var url = $"https://api.coingecko.com/api/v3/simple/price?ids={Uri.EscapeDataString(ids)}&vs_currencies={Uri.EscapeDataString(vsCurrencies)}";

                    using (var resp = await _http.GetAsync(url).ConfigureAwait(false))
                    {
                        resp.EnsureSuccessStatusCode();
                        using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                        using (var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false))
                        {
                            var root = doc.RootElement;
                            foreach (var kv in idMap)
                            {
                                var symbol = kv.Key;
                                var id = kv.Value;
                                if (root.TryGetProperty(id, out var coinEl) && coinEl.TryGetProperty("usd", out var usdEl))
                                {
                                    if (usdEl.ValueKind == JsonValueKind.Number && usdEl.TryGetDecimal(out var d))
                                        prices[symbol] = d;
                                    else if (usdEl.ValueKind == JsonValueKind.Number && usdEl.TryGetDouble(out var dd))
                                        prices[symbol] = (decimal)dd;
                                }
                            }
                        }
                    }
                }

                // 2) Yahoo Finance як резервний варіант для акцій / інших тікерів (без API-ключа)
                if (nonCrypto.Count > 0)
                {
                    // Yahoo дозволяє кілька символів, розділених комами
                    var chunkSize = 10; // уникати занадто довгих URL
                    for (int i = 0; i < nonCrypto.Count; i += chunkSize)
                    {
                        var chunk = nonCrypto.Skip(i).Take(chunkSize);
                        var q = string.Join(",", chunk);
                        var url = $"https://query1.finance.yahoo.com/v7/finance/quote?symbols={Uri.EscapeDataString(q)}";

                        try
                        {
                            using (var resp = await _http.GetAsync(url).ConfigureAwait(false))
                            {
                                resp.EnsureSuccessStatusCode();
                                using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                                using (var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false))
                                {
                                    if (doc.RootElement.TryGetProperty("quoteResponse", out var qr) &&
                                        qr.TryGetProperty("result", out var arr) &&
                                        arr.ValueKind == JsonValueKind.Array)
                                    {
                                        foreach (var el in arr.EnumerateArray())
                                        {
                                            if (el.TryGetProperty("symbol", out var symEl) && symEl.ValueKind == JsonValueKind.String &&
                                                el.TryGetProperty("regularMarketPrice", out var priceEl) &&
                                                (priceEl.ValueKind == JsonValueKind.Number || priceEl.ValueKind == JsonValueKind.String))
                                            {
                                                var sym = symEl.GetString().ToUpperInvariant();
                                                if (priceEl.TryGetDecimal(out var pd))
                                                    prices[sym] = pd;
                                                else if (priceEl.TryGetDouble(out var pdbl))
                                                    prices[sym] = (decimal)pdbl;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            // ігнорувати помилки для кожного блоку та продовжувати; помилки будуть повідомлені нижче, якщо нічого не вдасться отримати
                        }
                    }
                }

                // Викликати на UI-потоку з об'єднаними результатами (тільки якщо є щось)
                if (prices.Count > 0)
                {
                    Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                    {
                        PricesUpdated?.Invoke(prices);
                    }));
                }
                else
                {
                    // нічого не знайдено -> відобразити легке виключення, щоб дозволити UI логувати/показувати
                    Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
                        PricesUpdateFailed?.Invoke(new Exception("No prices returned from providers."))));
                }
            }
            catch (Exception ex)
            {
                Application.Current?.Dispatcher?.BeginInvoke(new Action(() => PricesUpdateFailed?.Invoke(ex)));
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void StartAutoRefresh(IEnumerable<string> symbols)
        {
            StopAutoRefresh();

            var snapshot = symbols?.Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim().ToUpperInvariant()).Distinct().ToList()
                            ?? new List<string>();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _refreshLoopTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await RefreshPricesAsync(snapshot).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        Application.Current?.Dispatcher?.BeginInvoke(new Action(() => PricesUpdateFailed?.Invoke(ex)));
                    }

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Max(5, RefreshIntervalSeconds)), token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException) { break; }
                }
            }, token);
        }

        public void StopAutoRefresh()
        {
            if (_cts == null) return;
            try
            {
                _cts.Cancel();
                _refreshLoopTask?.Wait(1000);
            }
            catch { }
            finally
            {
                _cts.Dispose();
                _cts = null;
                _refreshLoopTask = null;
            }
        }

        public void Dispose()
        {
            StopAutoRefresh();
            _http?.Dispose();
        }
    }
}