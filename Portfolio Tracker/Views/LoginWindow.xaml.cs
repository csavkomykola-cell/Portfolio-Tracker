using Portfolio_Tracker.Services;
using Portfolio_Tracker.Views;
using Portfolio_Tracker.Models;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Portfolio_Tracker.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += Window_PreviewKeyDown;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, 1, new System.Windows.Duration(System.TimeSpan.FromSeconds(0.18)));
            this.BeginAnimation(OpacityProperty, anim);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameText.Text?.Trim();
            var password = PasswordBox.Password;

            var (ok, message, user) = AuthService.Login(username, password);
            if (ok)
            {
                var main = new MainWindow(user);
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show((string)TryFindResource("InvalidCredentials") ?? message, (string)TryFindResource("ValidationError") ?? "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var reg = new RegisterWindow();
            reg.Owner = this;
            reg.ShowDialog();
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            var guest = AuthService.Guest();
            var main = new MainWindow(guest);
            main.Show();
            this.Close();
        }

        // Quick "cheat" — only when focus is NOT inside TextBox/PasswordBox
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.C)
            {
                var focused = FocusManager.GetFocusedElement(this);
                if (focused is System.Windows.Controls.TextBox || focused is System.Windows.Controls.PasswordBox)
                    return;

                var guest = AuthService.Guest();
                var main = new MainWindow(guest);
                main.Show();
                this.Close();
            }
        }
    }
}