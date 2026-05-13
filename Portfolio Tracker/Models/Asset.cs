using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Portfolio_Tracker.Models
{
    public class Asset : INotifyPropertyChanged, IDataErrorInfo
    {
        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _symbol;
        public string Symbol
        {
            get => _symbol;
            set
            {
                if (SetProperty(ref _symbol, value))
                    UpdateLastUpdated();
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                    UpdateLastUpdated();
            }
        }

        private string _assetType;
        public string AssetType
        {
            get => _assetType;
            set
            {
                if (SetProperty(ref _assetType, value))
                    UpdateLastUpdated();
            }
        }

        private string _currency;
        public string Currency
        {
            get => _currency;
            set
            {
                if (SetProperty(ref _currency, value))
                    UpdateLastUpdated();
            }
        }

        private decimal _currentPrice;
        public decimal CurrentPrice
        {
            get => _currentPrice;
            set
            {
                if (SetProperty(ref _currentPrice, value))
                    UpdateLastUpdated();
            }
        }

        private DateTime _lastUpdated;
        public DateTime LastUpdated
        {
            get => _lastUpdated;
            set => SetProperty(ref _lastUpdated, value);
        }

        public Asset()
        {
            Id = Guid.NewGuid();
            LastUpdated = DateTime.UtcNow;
        }

        private void UpdateLastUpdated()
        {
            LastUpdated = DateTime.UtcNow;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, value))
                return false;
            backingField = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
        #endregion

        #region IDataErrorInfo (simple validation)
        public string Error
        {
            get
            {
                var s = this[nameof(Symbol)];
                var n = this[nameof(Name)];
                var c = this[nameof(Currency)];
                if (!string.IsNullOrEmpty(s) || !string.IsNullOrEmpty(n) || !string.IsNullOrEmpty(c))
                {
                    return string.Join("; ", new[] { s, n, c }.Where(x => !string.IsNullOrEmpty(x)));
                }
                return null;
            }
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Symbol):
                        if (string.IsNullOrWhiteSpace(Symbol))
                            return "Symbol is required.";
                        break;
                    case nameof(Name):
                        if (string.IsNullOrWhiteSpace(Name))
                            return "Name is required.";
                        break;
                    case nameof(Currency):
                        if (string.IsNullOrWhiteSpace(Currency))
                            return "Currency is required.";
                        break;
                }
                return null;
            }
        }

        public bool Validate(out string errorMessage)
        {
            errorMessage = Error;
            return string.IsNullOrEmpty(errorMessage);
        }
        #endregion
    }
}