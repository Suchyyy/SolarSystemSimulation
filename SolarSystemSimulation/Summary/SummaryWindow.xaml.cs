using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using MahApps.Metro.Controls;
using SolarSystemSimulation.SolarSystem;

namespace SolarSystemSimulation.Summary
{
    public partial class SummaryWindow : MetroWindow
    {
        public Func<double, string> Formatter { get; }

        // ReSharper disable once SuggestBaseTypeForParameter
        public SummaryWindow(List<List<Point3D>> points, int sunCount = 1)
        {
            Formatter = d => d.ToString("0.##", CultureInfo.InvariantCulture);
            InitializeComponent();

            for (var i = 0; i < points.Count; i++)
            {
                var orbit = points[i];
                var series = new LineSeries
                {
                    Values = new ChartValues<ObservablePoint>(orbit.Select(point =>
                        new ObservablePoint(point.X / AstronomicalObject.Au, point.Z / AstronomicalObject.Au))),
                    Fill = Brushes.Transparent
                };

                if (sunCount == 1 && i == 0)
                    series.Title = "Gwiazda";
                else if (sunCount > 1 && i < sunCount)
                    series.Title = $"Gwiazda {(char) (65 + i)}";
                else
                    series.Title = $"Planeta {(char) (65 + i - sunCount)}";

                Chart.Series.Add(series);
            }
        }
    }
}