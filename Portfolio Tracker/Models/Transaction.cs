using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Portfolio_Tracker.Models
{
    public class Transaction : INotifyPropertyChanged, IDataErrorInfo
    {
        private Guid _id;
        public Guid Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private string _type;
        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        private string _asset;
        public string Asset
        {
            get => _asset;
            set
            {
                if (SetProperty(ref _asset, value))
                    OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private double _quantity;
        public double Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                    OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                    OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private decimal _fees;
        public decimal Fees
        {
            get => _fees;
            set
            {
                if (SetProperty(ref _fees, value))
                    OnPropertyChanged(nameof(TotalAmount));
            }
        }

        public decimal TotalAmount => (decimal)Quantity * Price + Fees;

        private DateTime _date;
        public DateTime Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }

        private string _notes;
        public string Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public Transaction()
        {
            Id = Guid.NewGuid();
            Date = DateTime.Now;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        protected bool SetProperty<T>(ref T backingField, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(backingField, value))
                return false;
            backingField = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region IDataErrorInfo
        public string Error
        {
            get
            {
                var a = this[nameof(Asset)];
                var q = this[nameof(Quantity)];
                var p = this[nameof(Price)];
                if (!string.IsNullOrEmpty(a) || !string.IsNullOrEmpty(q) || !string.IsNullOrEmpty(p))
                {
                    return string.Join("; ", new[] { a, q, p }.Where(x => !string.IsNullOrEmpty(x)));
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
                    case nameof(Asset):
                        if (string.IsNullOrWhiteSpace(Asset))
                            return "Asset is required.";
                        break;
                    case nameof(Quantity):
                        if (Quantity <= 0)
                            return "Quantity must be greater than zero.";
                        break;
                    case nameof(Price):
                        if (Price <= 0)
                            return "Price must be greater than zero.";
                        break;
                }
                return null;
            }
        }

        public bool Validate(out string message)
        {
            message = Error;
            return string.IsNullOrEmpty(message);
        }
        #endregion
    }
}