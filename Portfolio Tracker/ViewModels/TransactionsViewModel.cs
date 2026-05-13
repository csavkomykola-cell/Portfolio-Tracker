using Portfolio_Tracker.Models;
using Portfolio_Tracker.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace Portfolio_Tracker.ViewModels
{
    public class TransactionsViewModel : INotifyPropertyChanged
    {
        private const string Path = "Data/transactions.json";

        public ObservableCollection<Transaction> Transactions { get; set; }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedTransaction)));
            }
        }

        public TransactionsViewModel()
        {
            var loaded = JsonService.LoadTransactions<ObservableCollection<Transaction>>();

            Transactions = loaded ?? new ObservableCollection<Transaction>();

            if (Transactions.Count == 0)
            {
                Transactions.Add(new Transaction
                {
                    Type = "Buy",
                    Asset = "AAPL",
                    Quantity = 1,
                    Price = 100,
                    Fees = 0,
                    Notes = "",
                    Date = new DateTime(2026, 4, 1)
                });

                Save();
            }
        }

        public void Save()
        {
            JsonService.SaveTransactions(Transactions);
        }

        public bool ValidateTransaction(Transaction t, out string message)
        {
            if (t == null)
            {
                message = "Transaction is null.";
                return false;
            }
            return t.Validate(out message);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}