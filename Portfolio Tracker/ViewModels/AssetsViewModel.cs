using Portfolio_Tracker.Models;
using Portfolio_Tracker.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Collections.Generic;

namespace Portfolio_Tracker.ViewModels
{
    public class AssetsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Asset> Assets { get; set; }
        public ICollectionView AssetsView { get; private set; }

        private Asset _selectedAsset;
        public Asset SelectedAsset
        {
            get => _selectedAsset;
            set
            {
                _selectedAsset = value;
                OnPropertyChanged(nameof(SelectedAsset));
            }
        }

        private const string AssetsPath = "Data/assets.json";

        // Властивості пошуку, фільтрації та сортування
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery == value) return;
                _searchQuery = value;
                OnPropertyChanged(nameof(SearchQuery));
                AssetsView?.Refresh();
            }
        }

        public ObservableCollection<string> AssetTypes { get; } = new ObservableCollection<string>();

        private string _selectedAssetType = "All";
        public string SelectedAssetType
        {
            get => _selectedAssetType;
            set
            {
                if (_selectedAssetType == value) return;
                _selectedAssetType = value;
                OnPropertyChanged(nameof(SelectedAssetType));
                AssetsView?.Refresh();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string> { "Symbol", "Date", "Profit" };

        private string _selectedSortOption = "Symbol";
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption == value) return;
                _selectedSortOption = value;
                OnPropertyChanged(nameof(SelectedSortOption));
                ApplySort();
            }
        }

        public AssetsViewModel()
        {
            // Завантаження активів
            var loadedAssets = JsonService.LoadAssets<ObservableCollection<Asset>>();
            Assets = loadedAssets ?? new ObservableCollection<Asset>();

            // Переконатися, що id/останнє оновлення встановлені
            foreach (var a in Assets)
            {
                if (a.Id == Guid.Empty)
                    a.Id = Guid.NewGuid();
                if (a.LastUpdated == default)
                    a.LastUpdated = DateTime.UtcNow;
            }

            // Заповнення за замовчуванням, якщо порожньо
            if (Assets.Count == 0)
            {
                Assets.Add(new Asset
                {
                    Symbol = "AAPL",
                    Name = "Apple",
                    AssetType = "Акція",
                    Currency = "USD",
                    CurrentPrice = 0
                });

                Assets.Add(new Asset
                {
                    Symbol = "BTC",
                    Name = "Bitcoin",
                    AssetType = "Криптовалюта",
                    Currency = "USD",
                    CurrentPrice = 0
                });

                Assets.Add(new Asset
                {
                    Symbol = "TSLA",
                    Name = "Tesla",
                    AssetType = "Акція",
                    Currency = "USD",
                    CurrentPrice = 0
                });

                SaveAssets();
            }

            // Побудова списку типів активів на основі даних (включаючи "All")
            BuildAssetTypes();

            // Створення представлення та прив'язка фільтру та початкового сортування
            AssetsView = CollectionViewSource.GetDefaultView(Assets);
            AssetsView.Filter = AssetsFilter;
            ApplySort();
        }

        private void BuildAssetTypes()
        {
            AssetTypes.Clear();
            AssetTypes.Add("All");
            var types = Assets.Select(a => a.AssetType ?? string.Empty)
                              .Where(s => !string.IsNullOrWhiteSpace(s))
                              .Select(s => s.Trim())
                              .Distinct(StringComparer.OrdinalIgnoreCase)
                              .OrderBy(s => s);
            foreach (var t in types)
                AssetTypes.Add(t);

            // Переконатися, що SelectedAssetType дійсний
            if (!AssetTypes.Contains(SelectedAssetType))
                SelectedAssetType = "All";
        }

        private bool AssetsFilter(object obj)
        {
            if (!(obj is Asset asset))
                return false;

            // Фільтр за пошуком (символ або назва)
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.Trim().ToUpperInvariant();
                var sym = (asset.Symbol ?? string.Empty).ToUpperInvariant();
                var name = (asset.Name ?? string.Empty).ToUpperInvariant();
                if (!sym.Contains(q) && !name.Contains(q))
                    return false;
            }

            // Фільтр за типом активу
            if (!string.IsNullOrWhiteSpace(SelectedAssetType) && SelectedAssetType != "All")
            {
                if (!string.Equals((asset.AssetType ?? string.Empty).Trim(), SelectedAssetType.Trim(), StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private void ApplySort()
        {
            if (AssetsView == null) return;
            using (AssetsView.DeferRefresh())
            {
                AssetsView.SortDescriptions.Clear();
                // Відображення запитаних параметрів сортування на властивості:
                // "Symbol" -> Symbol
                // "Date" -> LastUpdated
                // "Profit" -> CurrentPrice (proxy) descending
                switch (SelectedSortOption)
                {
                    case "Symbol":
                        AssetsView.SortDescriptions.Add(new SortDescription(nameof(Asset.Symbol), ListSortDirection.Ascending));
                        break;
                    case "Date":
                        AssetsView.SortDescriptions.Add(new SortDescription(nameof(Asset.LastUpdated), ListSortDirection.Descending));
                        break;
                    case "Profit":
                        // Модель активу не відстежує прибуток на актив; використовуйте CurrentPrice як проксі для сортування.
                        AssetsView.SortDescriptions.Add(new SortDescription(nameof(Asset.CurrentPrice), ListSortDirection.Descending));
                        break;
                    default:
                        AssetsView.SortDescriptions.Add(new SortDescription(nameof(Asset.Symbol), ListSortDirection.Ascending));
                        break;
                }
            }
        }

        public void SaveAssets()
        {
            var validAssets = Assets
                .Where(a => a != null)
                .Where(a =>
                    !string.IsNullOrWhiteSpace(a.Symbol) &&
                    !string.IsNullOrWhiteSpace(a.Name) &&
                    !string.IsNullOrWhiteSpace(a.Currency))
                .ToList();

            JsonService.SaveAssets(validAssets);

            // Перебудова типів (у разі зміни AssetType) та оновлення представлення
            BuildAssetTypes();
            AssetsView?.Refresh();
        }

        public bool ValidateAsset(Asset asset, out string message)
        {
            if (asset == null)
            {
                message = "Asset is null.";
                return false;
            }

            var ok = asset.Validate(out message);
            return ok;
        }

        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
        }
    }
}