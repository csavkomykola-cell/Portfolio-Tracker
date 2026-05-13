using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Portfolio_Tracker.Models;
using Portfolio_Tracker.ViewModels;

namespace Portfolio_Tracker.Views
{
    /// <summary>
    /// Interaction logic for AssetsPage.xaml
    /// </summary>
    public partial class AssetsPage : Page
    {
        private AssetsViewModel vm;
        public AssetsPage()
        {
            InitializeComponent();
            vm = new AssetsViewModel();
            DataContext = vm;
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            // Використати діалогове вікно для створення нового активу (валідація всередині діалогу)
            var newAsset = new Asset();
            var window = new EditAssetWindow(newAsset);

            if (window.ShowDialog() == true)
            {
                // Переконатися, що Id встановлено
                if (newAsset.Id == Guid.Empty)
                    newAsset.Id = Guid.NewGuid();

                vm.Assets.Add(newAsset);
                vm.SaveAssets();
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedAsset == null)
            {
                MessageBox.Show("Оберіть актив");
                return;
            }

            var window = new EditAssetWindow(vm.SelectedAsset);

            if (window.ShowDialog() == true)
            {
                vm.SaveAssets();
                vm.OnPropertyChanged(nameof(vm.Assets));
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (vm.SelectedAsset != null)
            {
                if (MessageBox.Show("Видалити актив?", "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    vm.Assets.Remove(vm.SelectedAsset);
                    vm.SaveAssets();
                }
            }
            else
            {
                MessageBox.Show("Оберіть актив");
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

        private void AssetsGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            // Дозволити DataGrid спочатку зафіксувати зміни (commit), а потім перевірити та зберегти
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Row?.Item is Asset asset)
                {
                    if (vm.ValidateAsset(asset, out string message))
                    {
                        vm.SaveAssets();
                    }
                    else
                    {
                        MessageBox.Show(message, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }), System.Windows.Threading.DispatcherPriority.Background);
        }
    }
}