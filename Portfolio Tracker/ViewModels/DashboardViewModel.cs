using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Portfolio_Tracker.Models;

namespace Portfolio_Tracker.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly PortfolioViewModel _portfolioVm;

        public string TotalValue => _portfolioVm?.TotalValue.ToString("N2") + " $" ?? "0.00 $";
        public string TotalProfitLoss => (_portfolioVm?.TotalProfitLoss >= 0 ? "+" : "") + (_portfolioVm?.TotalProfitLoss.ToString("N2") ?? "0.00") + " $";
        public int AssetCount => _portfolioVm?.AssetCount ?? 0;

        public ObservableCollection<PortfolioItem> Assets { get; }

        public DashboardViewModel()
        {
            _portfolioVm = new PortfolioViewModel();
            _portfolioVm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PortfolioViewModel.TotalValue))
                    OnPropertyChanged(nameof(TotalValue));
                else if (e.PropertyName == nameof(PortfolioViewModel.TotalProfitLoss))
                    OnPropertyChanged(nameof(TotalProfitLoss));
                else if (e.PropertyName == nameof(PortfolioViewModel.AssetCount))
                    OnPropertyChanged(nameof(AssetCount));
            };

            // Повторно використати колекцію портфеля для відображення списку останніх активів
            Assets = _portfolioVm.Portfolio;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}