using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using DiffuseMaterial = HelixToolkit.Wpf.SharpDX.DiffuseMaterial;

namespace SolarSystemSimulation.SolarSystem
{
    public class SolarSystem
    {
        private static readonly MemoryStream SunTexture = TextureHelper.LoadFileToMemory(@"Resources\sun.jpg");

        public List<AstronomicalObject> Bodies { get; }
        public List<LineGeometryModel3D> OrbitModels { get; }
        public List<List<Point3D>> Orbits { get; }
        public List<LineBuilder> OrbitBuilders { get; }

        public bool IsRunning { get; set; }

        public SolarSystem(int stars, int planets)
        {
            const double au = AstronomicalObject.Au;
            const double me = AstronomicalObject.Me;

            Bodies = new List<AstronomicalObject>
            {
                new AstronomicalObject(new Point3D(0, 0, 0), 10.9 * 2) /* sun */
                {
                    Mass = 333_000 * me,
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial {DiffuseMap = SunTexture}
                }
            };

            if (stars > 1)
            {
                var mass = 333_000 * AstronomicalObject.Me * Utils.GetNormalRandom(0.3, 0.05);
                Bodies.Add(new AstronomicalObject(new Point3D(-AstronomicalObject.Au * 0.3, 0, 0), 10.9 * 2) /* sun */
                {
                    Mass = mass,
                    Velocity = new Vector3D(0, 0, -80e3),
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = SunTexture
                    }
                });
            }

            var centerMass = Bodies.Sum(body => body.Mass);

            for (var i = 1; i <= planets; i++)
            {
                var mass = Utils.GetNormalRandom(me * 2, me * 0.5);
                var position = new Point3D(Utils.GetNormalRandom(au * i / 2.0, au * 0.2), 0, 0);
                var velocity = GetVelocity(position.X, centerMass);
                var a = Utils.GetNormalRandom(0, Math.PI * 0.25);

                Bodies.Add(new AstronomicalObject(position, mass / me * 5)
                {
                    Mass = mass,
                    Velocity = new Vector3D(0, Math.Sin(a) * velocity, -Math.Cos(a) * velocity),
                    Material = new DiffuseMaterial
                    {
                        DiffuseColor = Utils.GetRandomColor4()
                    }
                });
            }

            Orbits = new List<List<Point3D>>();
            OrbitBuilders = new List<LineBuilder>();
            OrbitModels = new List<LineGeometryModel3D>();

            for (var i = 0; i < Bodies.Count; i++)
            {
                OrbitBuilders.Add(new LineBuilder());
                OrbitModels.Add(new LineGeometryModel3D
                {
                    Thickness = 0.5,
                    Color = Colors.Yellow
                });
                Orbits.Add(new List<Point3D>());
            }
        }

        public SolarSystem(List<AstronomicalObject> bodies)
        {
            Bodies = bodies;
            Orbits = new List<List<Point3D>>();
            OrbitBuilders = new List<LineBuilder>();
            OrbitModels = new List<LineGeometryModel3D>();

            for (var i = 0; i < Bodies.Count; i++)
            {
                OrbitBuilders.Add(new LineBuilder());
                OrbitModels.Add(new LineGeometryModel3D
                {
                    Thickness = 1,
                    Color = Colors.Yellow
                });
                Orbits.Add(new List<Point3D>());
            }
        }

        public async void StartSimulation(int frames, int dayScale)
        {
            IsRunning = true;

            var millisecondsDelay = 1000 / frames;
            var dt = 86_400.0 * dayScale / frames;
            var i = 1e9;

            const float scale = 1.0f / (float) AstronomicalObject.Au * 1000;

            while (IsRunning)
            {
                var task = Task.Delay(millisecondsDelay);

                if (i > frames / 15.0)
                {
                    i = 0;
                    for (var j = 0; j < Bodies.Count; j++)
                    {
                        var next = Bodies[j].Position;

                        Orbits[j].Add(next);

                        var builder = OrbitBuilders[j];
                        var model = OrbitModels[j];

                        var count = Orbits[j].Count;
                        if (count < 2) continue;

                        var prev = Orbits[j][count - 2];

                        builder.AddLine(prev.ToVector3() * scale, next.ToVector3() * scale);
                        model.Dispatcher?.Invoke(() => model.Geometry = builder.ToLineGeometry3D());
                    }
                }

                Bodies.ForEach(body => body.CalculateGravity(Bodies));
                Bodies.ForEach(body => body.UpdatePosition(dt));

                i++;

                await task;
            }
        }

        public static List<AstronomicalObject> GetSolarSystem()
        {
            return new List<AstronomicalObject>
            {
                new AstronomicalObject(new Point3D(0, 0, 0), 109 * 2) /* sun */
                {
                    Mass = 333_000 * AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = SunTexture
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
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 5.2, 0, 0), 112) /* jupiter */
                {
                    Mass = AstronomicalObject.Me * 317.87,
                    Velocity = new Vector3D(0, 0, -13.06e3),
                    Material = DiffuseMaterials.BlanchedAlmond,
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 9.52, 0, 0), 5.3) /* saturn */
                {
                    Mass = AstronomicalObject.Me * 95.14,
                    Velocity = new Vector3D(0, 0, -9.64e3),
                    Material = DiffuseMaterials.Bisque
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 19.16, 0, 0), 39.81) /* uranus */
                {
                    Mass = AstronomicalObject.Me * 14.56,
                    Velocity = new Vector3D(0, 0, -6.8e3),
                    Material = DiffuseMaterials.LightBlue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 30.06, 0, 0), 38.65) /* neptune */
                {
                    Mass = AstronomicalObject.Me * 17.21,
                    Velocity = new Vector3D(0, 0, -5.43e3),
                    Material = DiffuseMaterials.Blue
                }
            };
        }

        private static double GetVelocity(double radius, double centerMass)
        {
            return Math.Sqrt(AstronomicalObject.G * centerMass / radius);
        }
    }
}