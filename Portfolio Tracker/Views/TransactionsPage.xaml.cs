using Portfolio_Tracker.Models;
using Portfolio_Tracker.ViewModels;
using Portfolio_Tracker.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class TransactionsPage : Page
    {
        private TransactionsViewModel vm;
        public TransactionsPage()
        {
            InitializeComponent();
            vm = new TransactionsViewModel();
            DataContext = vm;
        }

        private void AddTransaction_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddTransactionWindow();
            if (window.ShowDialog() == true)
            {
                vm.Transactions.Add(window.Transaction);
                vm.Save();
            }
        }

        private void EditTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedTransaction == null)
            {
                MessageBox.Show((string)TryFindResource("SelectTransaction") ?? "Select a transaction");
                return;
            }

            var copy = vm.SelectedTransaction;
            var window = new AddTransactionWindow(copy);
            if (window.ShowDialog() == true)
            {
                // changes already applied to the object via the dialog
                vm.Save();
            }
        }

        private void DeleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedTransaction == null)
            {
                MessageBox.Show((string)TryFindResource("SelectTransaction") ?? "Select a transaction");
                return;
            }

            if (MessageBox.Show((string)TryFindResource("ConfirmDelete") ?? "Delete transaction?", (string)TryFindResource("Confirmation") ?? "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                vm.Transactions.Remove(vm.SelectedTransaction);
                vm.Save();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2)
            };

            this.BeginAnimation(OpacityProperty, animation);
        }

        private void TransactionsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Row?.Item is Transaction t)
                {
                    if (vm.ValidateTransaction(t, out string message))
                    {
                        vm.Save();
                    }
                    else
                    {
                        MessageBox.Show(message, (string)TryFindResource("ValidationError") ?? "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}