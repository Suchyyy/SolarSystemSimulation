using System;
using System.Globalization;
using System.Windows.Data;
using SolarSystemSimulation.SolarSystem;

namespace SolarSystemSimulation
{
    public class MassConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0";

            var mass = double.Parse(value.ToString());

            return $"{(mass / AstronomicalObject.Me):0.00} M⊕";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var massString = value.ToString().Split(' ')[0];

            return double.Parse(massString) * AstronomicalObject.Me;
        }
    }
}