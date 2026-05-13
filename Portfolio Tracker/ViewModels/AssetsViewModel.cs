using Portfolio_Tracker.Models;
using Portfolio_Tracker.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Portfolio_Tracker.ViewModels
{
    public class AssetsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<Asset> Assets { get; set; }
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

        public AssetsViewModel()
        {
            // Завантаження з JSON за допомогою централізованого сервісу JsonService
            var loadedAssets = JsonService.LoadAssets<ObservableCollection<Asset>>();

            Assets = loadedAssets ?? new ObservableCollection<Asset>();

            // Переконатися, що існуючі активи мають встановлені Ids та LastUpdated
            foreach (var a in Assets)
            {
                if (a.Id == Guid.Empty)
                    a.Id = Guid.NewGuid();
                if (a.LastUpdated == default)
                    a.LastUpdated = DateTime.UtcNow;
            }

            // Якщо файл порожній — додати початкові дані
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
        }

        public void SaveAssets()
        {
            // Перевірити (валідувати) та зберегти лише коректні активи
            var validAssets = Assets
                .Where(a => a != null)
                .Where(a =>
                    !string.IsNullOrWhiteSpace(a.Symbol) &&
                    !string.IsNullOrWhiteSpace(a.Name) &&
                    !string.IsNullOrWhiteSpace(a.Currency))
                .ToList();

            JsonService.SaveAssets(validAssets);
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

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this,
                new PropertyChangedEventArgs(name));
        }
    }
}