using Portfolio_Tracker.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class AddTransactionWindow : Window
    {
        public Transaction Transaction { get; private set; }

        public AddTransactionWindow()
        {
            InitializeComponent();
            Transaction = new Transaction();
            DatePicker.SelectedDate = Transaction.Date;
            UpdateTotal();
        }

        public AddTransactionWindow(Transaction existing) : this()
        {
            if (existing != null)
            {
                Transaction = existing;
                AssetText.Text = Transaction.Asset;
                QuantityText.Text = Transaction.Quantity.ToString(CultureInfo.InvariantCulture);
                PriceText.Text = Transaction.Price.ToString(CultureInfo.InvariantCulture);
                FeesText.Text = Transaction.Fees.ToString(CultureInfo.InvariantCulture);
                NotesText.Text = Transaction.Notes;
                DatePicker.SelectedDate = Transaction.Date;
                // Встановити значення TypeCombo
                foreach (var item in TypeCombo.Items)
                {
                    if (item is System.Windows.Controls.ComboBoxItem c && (string)c.Content == Transaction.Type)
                    {
                        TypeCombo.SelectedItem = item;
                        break;
                    }
                }
                UpdateTotal();
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.18)
            };

            this.BeginAnimation(OpacityProperty, animation);

            // Підключити обробники подій зміни (change handlers)
            QuantityText.TextChanged += (_, __) => UpdateTotal();
            PriceText.TextChanged += (_, __) => UpdateTotal();
            FeesText.TextChanged += (_, __) => UpdateTotal();
        }

        private void UpdateTotal()
        {
            if (!double.TryParse(QuantityText?.Text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out double qty))
                qty = 0;
            if (!decimal.TryParse(PriceText?.Text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                price = 0;
            if (!decimal.TryParse(FeesText?.Text ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out decimal fees))
                fees = 0;

            var total = (decimal)qty * price + fees;
            TotalAmountText.Text = total.ToString("N2", CultureInfo.InvariantCulture);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Прочитати значення та виконати валідацію
            var typeItem = TypeCombo.SelectedItem as System.Windows.Controls.ComboBoxItem;
            Transaction.Type = typeItem?.Content?.ToString() ?? "Buy";
            Transaction.Asset = AssetText.Text?.Trim() ?? string.Empty;

            if (!double.TryParse(QuantityText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out double quantity))
            {
                MessageBox.Show((string)TryFindResource("InvalidQuantity") ?? "Invalid quantity.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Transaction.Quantity = quantity;

            if (!decimal.TryParse(PriceText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
            {
                MessageBox.Show((string)TryFindResource("InvalidPrice") ?? "Invalid price.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Transaction.Price = price;

            if (!decimal.TryParse(FeesText.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal fees))
            {
                MessageBox.Show((string)TryFindResource("InvalidFees") ?? "Invalid fees.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Transaction.Fees = fees;

            Transaction.Notes = NotesText.Text ?? string.Empty;
            Transaction.Date = DatePicker.SelectedDate ?? DateTime.Now;

            if (!Transaction.Validate(out string msg))
            {
                MessageBox.Show(msg, (string)TryFindResource("ValidationError") ?? "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}