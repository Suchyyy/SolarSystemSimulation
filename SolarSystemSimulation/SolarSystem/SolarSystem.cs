using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;

namespace SolarSystemSimulation.SolarSystem
{
    public class SolarSystem
    {
        public List<AstronomicalObject> Bodies { get; set; }
        public List<List<Point>> Orbits { get; set; }

        public bool IsRunning { get; set; }

        public SolarSystem()
        {
            Bodies = new List<AstronomicalObject>
            {
                new AstronomicalObject(new Point3D(0, 0, 0), 109 * 2) /* sun */
                {
                    Mass = 333_000 * AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = TextureHelper.LoadFileToMemory(@"Resources\sun.jpg")
                    }
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.39, 0, 0), 3.8) /* mercury */
                {
                    Mass = AstronomicalObject.Me * 0.06,
                    Velocity = new Vector3D(0, 0, -47.89e3),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.72, 0, 0), 9.5) /* venus */
                {
                    Mass = AstronomicalObject.Me * 0.81,
                    Velocity = new Vector3D(0, 0, -35e3),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au, 0, 0), 10) /* earth */
                {
                    Mass = AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, -30e3),
                    Material = DiffuseMaterials.Blue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 1.52, 0, 0), 5.3) /* mars */
                {
                    Mass = AstronomicalObject.Me * 0.11,
                    Velocity = new Vector3D(0, 0, -24.13e3),
                    Material = DiffuseMaterials.Copper
                }
            };

            Orbits = new List<List<Point>>();
            for (var i = 0; i < Bodies.Count; i++)
            {
                Orbits.Add(new List<Point>());
            }
        }

        public async void StartSimulation(int frames, int dayScale)
        {
            IsRunning = true;

            var millisecondsDelay = 1000 / frames;
            var dt = 86_400.0 * dayScale / frames;
            var i = 100;

            while (IsRunning)
            {
                var task = Task.Delay(millisecondsDelay);

                if (i > 50)
                {
                    i = 0;
                    for (var i1 = 0; i1 < Bodies.Count; i1++)
                    {
                        var point3d = Bodies[i1].Position;
                        Orbits[i1].Add(new Point(point3d.X / AstronomicalObject.Au, point3d.Z / AstronomicalObject.Au));
                    }
                }

                Bodies.ForEach(body => body.CalculateGravity(Bodies));
                Bodies.ForEach(body => body.UpdatePosition(dt));

                i++;

                await task;
            }
        }
    }
}