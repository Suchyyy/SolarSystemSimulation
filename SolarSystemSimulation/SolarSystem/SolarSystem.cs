using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
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
        public List<LineBuilder> OrbitBuilders { get; }
        public List<Point3D> OrbitPrevPoints { get; }

        public bool IsRunning { get; set; }

        public SolarSystem(int stars, int planets)
        {
            const double au = AstronomicalObject.Au;
            const double me = AstronomicalObject.Me;

            Bodies = new List<AstronomicalObject>
            {
                new AstronomicalObject(new Point3D(0, 0, 0), 10.9 * 2) /* sun */
                {
                    BodyName = stars == 1 ? "Gwiazda" : "Gwiazda A",
                    Mass = 333_000 * me * Utils.GetNormalRandom(10, 2),
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial {DiffuseMap = SunTexture}
                }
            };

            if (stars > 1)
            {
                var mass = 333_000 * AstronomicalObject.Me * Utils.GetNormalRandom(1, 0.2);
                var position = -AstronomicalObject.Au * 0.2;

                Bodies.Add(new AstronomicalObject(new Point3D(position, 0, 0), 10.9) /* sun */
                {
                    BodyName = "Gwiazda B",
                    Mass = mass,
                    Velocity = new Vector3D(0, 0, -GetVelocity(Math.Abs(position), Bodies[0].Mass + mass)) * 1.1,
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = SunTexture
                    }
                });

                Bodies[0].Velocity = -Bodies[1].Velocity * mass / Bodies[0].Mass;
            }

            var centerMass = Bodies.Sum(body => body.Mass);

            for (var i = 1; i <= planets; i++)
            {
                var mass = Utils.GetNormalRandom(me * 2, me * 0.5);
                var position = new Point3D(Utils.GetNormalRandom(au * i / 2.0, au * 0.2), 0, 0);
                var velocity = GetVelocity(position.X, centerMass);
                var a = Utils.GetNormalRandom(0, Math.PI * 0.05);

                Bodies.Add(new AstronomicalObject(position, mass / me * 5)
                {
                    BodyName = $"Planeta {(char) (64 + i)}",
                    Mass = mass,
                    Velocity = new Vector3D(0, Math.Sin(a) * velocity, -Math.Cos(a) * velocity),
                    Material = new DiffuseMaterial
                    {
                        DiffuseColor = Utils.GetRandomColor4()
                    }
                });
            }

            OrbitPrevPoints = new List<Point3D>();
            OrbitBuilders = new List<LineBuilder>();
            OrbitModels = new List<LineGeometryModel3D>();

            foreach (var body in Bodies)
            {
                OrbitBuilders.Add(new LineBuilder());
                OrbitModels.Add(new LineGeometryModel3D
                {
                    Thickness = 0.5,
                    Color = Colors.Yellow
                });
                OrbitPrevPoints.Add(body.Position);
            }
        }

        public SolarSystem(List<AstronomicalObject> bodies)
        {
            Bodies = bodies;
            OrbitPrevPoints = new List<Point3D>();
            OrbitBuilders = new List<LineBuilder>();
            OrbitModels = new List<LineGeometryModel3D>();

            foreach (var body in Bodies)
            {
                OrbitBuilders.Add(new LineBuilder());
                OrbitModels.Add(new LineGeometryModel3D
                {
                    Thickness = 0.5,
                    Color = Colors.Yellow
                });
                OrbitPrevPoints.Add(body.Position);
            }
        }

        public async void StartSimulation(int frames, int dayScale)
        {
            IsRunning = true;

            var sw = new Stopwatch();
            sw.Start();

            var frameTime = 1000 / frames;

            var last = sw.ElapsedMilliseconds;
            var delta = 0.0;
            var i = 1e9;

            const float scale = 1.0f / (float) AstronomicalObject.Au * 1000;

            while (IsRunning)
            {
                var now = sw.ElapsedMilliseconds;
                delta = now - last;

                if (i > 4)
                {
                    i = 0;
                    for (var j = 0; j < Bodies.Count; j++)
                    {
                        var next = Bodies[j].Position;

                        var builder = OrbitBuilders[j];
                        var model = OrbitModels[j];

                        var prev = OrbitPrevPoints[j];
                        OrbitPrevPoints[j] = next;

                        builder.AddLine(prev.ToVector3() * scale, next.ToVector3() * scale);
                        model.Dispatcher?.Invoke(() => model.Geometry = builder.ToLineGeometry3D());
                    }
                }

                Bodies.ForEach(body => body.CalculateGravity(Bodies));
                Bodies.ForEach(body => body.UpdatePosition(86_400.0 * dayScale / frames));

                last = now;

                i++;

                if (delta < frameTime)
                    Thread.Sleep(frameTime - (int) delta);
            }
        }

        public static List<AstronomicalObject> GetSolarSystem()
        {
            return new List<AstronomicalObject>
            {
                new AstronomicalObject(new Point3D(0, 0, 0), 109 * 2)
                {
                    BodyName = "Słońce",
                    Mass = 333_000 * AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = SunTexture
                    }
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.39, 0, 0), 3.8)
                {
                    BodyName = "Merkury",
                    Mass = AstronomicalObject.Me * 0.06,
                    Velocity =
                        new Vector3D(0, -47.89e3 * Math.Sin(ToRadians(7.0)), -47.89e3 * Math.Cos(ToRadians(7.0))),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.72, 0, 0), 9.5)
                {
                    BodyName = "Wenus",
                    Mass = AstronomicalObject.Me * 0.81,
                    Velocity = new Vector3D(0, -35e3 * Math.Sin(ToRadians(3.4)), -35e3 * Math.Cos(ToRadians(3.4))),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au, 0, 0), 10)
                {
                    BodyName = "Ziemia",
                    Mass = AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, -30e3),
                    Material = DiffuseMaterials.Blue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 1.52, 0, 0), 5.3)
                {
                    BodyName = "Mars",
                    Mass = AstronomicalObject.Me * 0.11,
                    Velocity =
                        new Vector3D(0, -24.13e3 * Math.Sin(ToRadians(1.9)), -24.13e3 * Math.Cos(ToRadians(1.9))),
                    Material = DiffuseMaterials.Copper
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 5.2, 0, 0), 112)
                {
                    BodyName = "Jowisz",
                    Mass = AstronomicalObject.Me * 317.87,
                    Velocity =
                        new Vector3D(0, -13.06e3 * Math.Sin(ToRadians(1.3)), -13.06e3 * Math.Cos(ToRadians(1.3))),
                    Material = DiffuseMaterials.BlanchedAlmond,
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 9.52, 0, 0), 5.3)
                {
                    BodyName = "Saturn",
                    Mass = AstronomicalObject.Me * 95.14,
                    Velocity = new Vector3D(0, -9.64e3 * Math.Sin(ToRadians(2.5)), -9.64e3 * Math.Cos(ToRadians(2.5))),
                    Material = DiffuseMaterials.Bisque
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 19.16, 0, 0), 39.81)
                {
                    BodyName = "Uran",
                    Mass = AstronomicalObject.Me * 14.56,
                    Velocity = new Vector3D(0, -6.8e3 * Math.Sin(ToRadians(0.8)), -6.8e3 * Math.Cos(ToRadians(0.8))),
                    Material = DiffuseMaterials.LightBlue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 30.06, 0, 0), 38.65)
                {
                    BodyName = "Neptun",
                    Mass = AstronomicalObject.Me * 17.21,
                    Velocity = new Vector3D(0, -5.43e3 * Math.Sin(ToRadians(1.8)), -5.43e3 * Math.Cos(ToRadians(1.8))),
                    Material = DiffuseMaterials.Blue
                }
            };
        }

        private static double GetVelocity(double distance, double centerMass)
        {
            return Math.Sqrt(AstronomicalObject.G * centerMass / distance);
        }

        private static double ToRadians(double angle)
        {
            return angle * Math.PI / 180.0;
        }
    }
}