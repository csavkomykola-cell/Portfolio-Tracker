using Portfolio_Tracker.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    /// <summary>
    /// Interaction logic for CryptoMarketPage.xaml
    /// </summary>
    public partial class CryptoMarketPage : Page
    {
        public CryptoMarketPage()
        {
            InitializeComponent();
            DataContext = new CryptoMarketViewModel();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation { From = 0, To = 1, Duration = System.TimeSpan.FromSeconds(0.2) };
            this.BeginAnimation(OpacityProperty, animation);
        }
    }
}
