using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SharpDX;
using Color = System.Drawing.Color;
using NewColor = System.Windows.Media.Color;

namespace SolarSystemSimulation
{
    public static class Utils
    {
        private static readonly Random Random = new Random();

        private static readonly List<KnownColor> KnownColors = Enum.GetValues(typeof(KnownColor))
            .Cast<KnownColor>()
            .Where(color => color != KnownColor.Transparent)
            .ToList();

        public static double GetNormalRandom(double mean, double std)
        {
            var u1 = 1.0 - Random.NextDouble();
            var u2 = 1.0 - Random.NextDouble();

            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            return mean + randStdNormal * std;
        }

        public static NewColor GetRandomColor()
        {
            var index = Random.Next(KnownColors.Count);
            var color = Color.FromKnownColor(KnownColors[index]);

            return NewColor.FromRgb(color.R, color.G, color.B);
        }

        public static Color4 GetRandomColor4()
        {
            var index = Random.Next(KnownColors.Count);
            var color = Color.FromKnownColor(KnownColors[index]);

            return new Color4(color.R / 255f, color.G / 255f, color.B / 255f, color.A / 255f);
        }

        public static double GetNextDouble(double max)
        {
            return Random.NextDouble() * max;
        }
    }
}