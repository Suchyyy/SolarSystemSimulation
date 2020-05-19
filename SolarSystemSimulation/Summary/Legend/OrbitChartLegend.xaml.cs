using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts.Wpf;
using LiveCharts.Wpf.Charts.Base;

namespace SolarSystemSimulation.Summary.Legend
{
    public sealed partial class OrbitChartLegend : UserControl, IChartLegend
    {
        public ObservableCollection<OrbitSeriesViewModel> Legend { get; } =
            new ObservableCollection<OrbitSeriesViewModel>();

        public List<SeriesViewModel> Series
        {
            get => Legend.Select(x => x.SeriesViewModel).ToList();
            set
            {
                var owner = FindParent<Chart>(this);

                var removedSeries = Legend.Where(x => owner.Series.All(s => s != x.View))
                    .ToList();

                removedSeries.ForEach(s => Legend.Remove(s));

                foreach (var seriesViewModel in value)
                {
                    if (Legend.Any(x => x.Title == seriesViewModel.Title)) continue;

                    var view = owner.Series.FirstOrDefault(x => x.Title == seriesViewModel.Title);
                    Legend.Add(new OrbitSeriesViewModel(seriesViewModel, view));
                }

                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(OrbitChartLegend));

        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public OrbitChartLegend()
        {
            InitializeComponent();

            ItemsControl.DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (true)
            {
                var parentObject = VisualTreeHelper.GetParent(child);

                switch (parentObject)
                {
                    case null:
                        return null;
                    case T parent:
                        return parent;
                    default:
                        child = parentObject;
                        break;
                }
            }
        }
    }
}