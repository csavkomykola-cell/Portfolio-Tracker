using Portfolio_Tracker.Models;
using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class EditAssetWindow : Window
    {
        public Asset Asset { get; set; }

        public EditAssetWindow(Asset asset)
        {
            InitializeComponent();
            Asset = asset ?? new Asset();
            DataContext = Asset;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!Asset.Validate(out string message))
            {
                MessageBox.Show(message, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
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
    }
}