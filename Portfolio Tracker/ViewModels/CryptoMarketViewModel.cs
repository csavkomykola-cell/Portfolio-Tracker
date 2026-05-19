using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;

namespace Portfolio_Tracker.ViewModels
{
    public class CryptoCoin
    {
        public string Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Change24h { get; set; }
    }

    public class CryptoMarketViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
        public ObservableCollection<CryptoCoin> Coins { get; } = new ObservableCollection<CryptoCoin>();

        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(nameof(SearchQuery)); Filter(); }
        }

        private CryptoCoin _selectedCoin;
        public CryptoCoin SelectedCoin { get => _selectedCoin; set { _selectedCoin = value; OnPropertyChanged(nameof(SelectedCoin)); } }

        private DateTime? _lastUpdated;
        public string LastUpdatedDisplay => _lastUpdated.HasValue ? $"Updated: {_lastUpdated:HH:mm:ss}" : "";

        public ICommand RefreshCommand { get; }

        public CryptoMarketViewModel()
        {
            RefreshCommand = new RelayCommand(async _ => await LoadAsync());
            _ = LoadAsync();
        }

        private ObservableCollection<CryptoCoin> _all = new ObservableCollection<CryptoCoin>();

        public async Task LoadAsync()
        {
            try
            {
                // Ендпоінт ринків CoinGecko (markets endpoint)
                var url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=250&page=1&sparkline=false&price_change_percentage=24h";
                using (var resp = await _http.GetAsync(url).ConfigureAwait(false))
                {
                    resp.EnsureSuccessStatusCode();
                    using (var stream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var doc = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
                        var arr = doc.RootElement;
                        _all.Clear();
                        foreach (var el in arr.EnumerateArray())
                        {
                            var coin = new CryptoCoin
                            {
                                Id = el.GetProperty("id").GetString(),
                                Symbol = (el.GetProperty("symbol").GetString() ?? "").ToUpperInvariant(),
                                Name = el.GetProperty("name").GetString(),
                                ImageUrl = el.GetProperty("image").GetString(),
                                CurrentPrice = el.TryGetProperty("current_price", out var p) && p.TryGetDecimal(out var dp) ? dp : 0,
                                MarketCap = el.TryGetProperty("market_cap", out var m) && m.TryGetDecimal(out var dm) ? dm : 0,
                                Change24h = el.TryGetProperty("price_change_percentage_24h", out var c) && c.TryGetDecimal(out var dc) ? dc : 0
                            };
                            _all.Add(coin);
                        }
                    }
                }

                _lastUpdated = DateTime.Now;
                OnPropertyChanged(nameof(LastUpdatedDisplay));
                Filter();
            }
            catch (Exception)
            {
                // проглотить — UI может выбрать отображение ошибок позже
            }
        }

        private void Filter()
        {
            Application.Current?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                Coins.Clear();
                var q = (SearchQuery ?? "").Trim().ToLowerInvariant();
                foreach (var c in _all)
                {
                    if (string.IsNullOrEmpty(q) || c.Name.ToLowerInvariant().Contains(q) || c.Symbol.ToLowerInvariant().Contains(q))
                        Coins.Add(c);
                }
            }));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    // Проста команда RelayCommand
    public class RelayCommand : ICommand
    {
        private readonly Func<object, Task> _execAsync;
        private readonly Action<object> _exec;
        private readonly Predicate<object> _canExec;

        public RelayCommand(Func<object, Task> execAsync, Predicate<object> canExec = null) { _execAsync = execAsync; _canExec = canExec; }
        public RelayCommand(Action<object> exec, Predicate<object> canExec = null) { _exec = exec; _canExec = canExec; }

        public bool CanExecute(object parameter) => _canExec?.Invoke(parameter) ?? true;

        public async void Execute(object parameter)
        {
            if (_execAsync != null) await _execAsync(parameter).ConfigureAwait(false);
            else _exec?.Invoke(parameter);
        }

        // Підписатися на CommandManager.RequerySuggested, щоб використати подію та прибрати попередження
        public event EventHandler CanExecuteChanged
        {
            add { System.Windows.Input.CommandManager.RequerySuggested += value; }
            remove { System.Windows.Input.CommandManager.RequerySuggested -= value; }
        }
    }
}