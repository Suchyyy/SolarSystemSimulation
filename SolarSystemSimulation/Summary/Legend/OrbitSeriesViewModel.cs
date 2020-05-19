using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using LiveCharts.Definitions.Series;
using LiveCharts.Wpf;

namespace SolarSystemSimulation.Summary.Legend
{
    public sealed class OrbitSeriesViewModel : INotifyPropertyChanged
    {
        public SeriesViewModel SeriesViewModel { get; }

        public string Title => SeriesViewModel.Title;
        public Brush Fill => SeriesViewModel.Stroke ?? SeriesViewModel.Fill;
        public ISeriesView View { get; }

        public bool IsVisible
        {
            get => ((UIElement) View).Visibility == Visibility.Visible;
            set
            {
                if (IsVisible == value) return;

                ((UIElement) View).Visibility = value ? Visibility.Visible : Visibility.Hidden;
                OnPropertyChanged();
            }
        }

        public OrbitSeriesViewModel(SeriesViewModel seriesViewModel, ISeriesView view)
        {
            SeriesViewModel = seriesViewModel;
            View = view;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}