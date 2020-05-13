using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using Brushes = System.Windows.Media.Brushes;
using Point = System.Windows.Point;

namespace SolarSystemSimulation
{
    public partial class OrbitWindow : MetroWindow
    {
        public Func<double, double> Formatter { get; set; }

        // ReSharper disable once SuggestBaseTypeForParameter
        public OrbitWindow(List<List<Point>> points, int sunCount = 1)
        {
            Formatter = d => Math.Round(d, 2);

            InitializeComponent();

            for (var i = 0; i < points.Count; i++)
            {
                var orbit = points[i];
                var series = new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>(orbit.Select(point =>
                        new ObservablePoint(point.X, point.Y))),
                    Fill = Brushes.Transparent
                };

                if (sunCount == 1 && i == 0)
                {
                    series.Title = "Gwiazda";
                }
                else if (sunCount > 1 && i < sunCount)
                {
                    series.Title = $"Gwiazda {(char) (65 + i)}";
                }
                else
                {
                    series.Title = $"Planeta {(char) (65 + i - sunCount)}";
                }

                Chart.Series.Add(series);
            }
        }
    }
}