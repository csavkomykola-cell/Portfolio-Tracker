using Portfolio_Tracker.Views;
using Portfolio_Tracker.Models;
using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class MainWindow : Window
    {
        private readonly User _currentUser;

        public MainWindow(User currentUser = null)
        {
            InitializeComponent();

            _currentUser = currentUser;
            MainFrame.Navigated += MainFrame_Navigated;

            ApplyUserState();
        }

        private void ApplyUserState()
        {
            // Гість: показувати лише "Налаштування", "Крипторинок" та "Вихід"
            if (_currentUser?.IsGuest == true)
            {
                BtnDashboard.Visibility = Visibility.Collapsed;
                BtnPortfolio.Visibility = Visibility.Collapsed;
                BtnTransactions.Visibility = Visibility.Collapsed;
                BtnAssets.Visibility = Visibility.Collapsed;

                BtnCryptoMarket.Visibility = Visibility.Visible;
                BtnSettings.Visibility = Visibility.Visible;

                MainFrame.Navigate(new CryptoMarketPage());
                SetActiveToggle(BtnCryptoMarket);
            }
            else
            {
                BtnDashboard.Visibility = Visibility.Visible;
                BtnPortfolio.Visibility = Visibility.Visible;
                BtnTransactions.Visibility = Visibility.Visible;
                BtnAssets.Visibility = Visibility.Visible;
                BtnCryptoMarket.Visibility = Visibility.Visible;
                BtnSettings.Visibility = Visibility.Visible;

                MainFrame.Navigate(new DashboardPage());
                SetActiveToggle(BtnDashboard);
            }
        }

        private void SetActiveToggle(ToggleButton active)
        {
            BtnDashboard.IsChecked = false;
            BtnPortfolio.IsChecked = false;
            BtnTransactions.IsChecked = false;
            BtnAssets.IsChecked = false;
            BtnCryptoMarket.IsChecked = false;
            BtnSettings.IsChecked = false;

            if (active != null)
                active.IsChecked = true;
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnDashboard);
            MainFrame.Navigate(new DashboardPage());
        }

        private void Portfolio_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnPortfolio);
            MainFrame.Navigate(new PortfolioPage());
        }

        private void Transactions_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnTransactions);
            MainFrame.Navigate(new TransactionsPage());
        }

        private void Assets_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnAssets);
            MainFrame.Navigate(new AssetsPage());
        }

        private void CryptoMarket_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnCryptoMarket);
            MainFrame.Navigate(new CryptoMarketPage());
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnSettings);
            MainFrame.Navigate(new SettingsPage());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                (string)TryFindResource("ExitConfirmation") ?? "Are you sure you want to exit?",
                (string)TryFindResource("Confirm") ?? "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
                Application.Current.Shutdown();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.28)
            };
            this.BeginAnimation(OpacityProperty, animation);
        }

        private void MainFrame_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Content is FrameworkElement fe)
            {
                fe.RenderTransform = new System.Windows.Media.TranslateTransform(40, 0);
                fe.Opacity = 0;

                var sb = new Storyboard();
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.28))
                {
                    EasingFunction = new QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                Storyboard.SetTarget(fade, fe);
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));

                var slide = new DoubleAnimation(40, 0, TimeSpan.FromSeconds(0.28))
                {
                    EasingFunction = new QuadraticEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
                };
                Storyboard.SetTarget(slide, fe);
                Storyboard.SetTargetProperty(slide, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                sb.Children.Add(fade);
                sb.Children.Add(slide);
                sb.Begin();
            }
        }
    }
}