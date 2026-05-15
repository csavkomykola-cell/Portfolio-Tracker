using Portfolio_Tracker.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // початкова навігація
            MainFrame.Navigated += MainFrame_Navigated;
            MainFrame.Navigate(new DashboardPage());
            BtnDashboard.IsChecked = true;
        }

        private void SetActiveToggle(ToggleButton active)
        {
            BtnDashboard.IsChecked = false;
            BtnPortfolio.IsChecked = false;
            BtnTransactions.IsChecked = false;
            BtnAssets.IsChecked = false;
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

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            SetActiveToggle(BtnSettings);
            MainFrame.Navigate(new SettingsPage());
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
            // Застосувати анімацію зсуву та згасання (slide+fade) до контенту нової сторінки
            if (e.Content is FrameworkElement fe)
            {
                fe.RenderTransform = new TranslateTransform(40, 0);
                fe.Opacity = 0;

                var sb = new Storyboard();
                var fade = new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.28)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(fade, fe);
                Storyboard.SetTargetProperty(fade, new PropertyPath("Opacity"));

                var slide = new DoubleAnimation(40, 0, TimeSpan.FromSeconds(0.28)) { EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut } };
                Storyboard.SetTarget(slide, fe);
                Storyboard.SetTargetProperty(slide, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

                sb.Children.Add(fade);
                sb.Children.Add(slide);
                sb.Begin();
            }
        }
    }
}