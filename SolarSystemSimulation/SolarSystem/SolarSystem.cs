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
                var position = -AstronomicalObject.Au * 0.3;

                Bodies.Add(new AstronomicalObject(new Point3D(position, 0, 0), 10.9) /* sun */
                {
                    BodyName = "Gwiazda B",
                    Mass = mass,
                    Velocity = new Vector3D(0, 0, -GetVelocity(Math.Abs(position), Bodies[0].Mass + mass)) * 1.15,
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
                var mass = Utils.GetNormalRandom(me * 2, me * 0.8);
                var position = new Point3D(Utils.GetNormalRandom(au * i * 0.7, au * 0.1), 0, 0);
                var velocity = GetVelocity(position.X, centerMass) * Utils.GetNormalRandom(1.1, 0.1);
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

        public void StartSimulation(int frames, int dayScale)
        {
            IsRunning = true;

            var sw = new Stopwatch();
            sw.Start();

            var frameTime = 1000 / frames;

            var last = sw.ElapsedMilliseconds;
            var i = 1e9;

            const float scale = 1.0f / (float) AstronomicalObject.Au * 1000;

            while (IsRunning)
            {
                var now = sw.ElapsedMilliseconds;
                double delta = now - last;

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
                    Mass = 332_950 * AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, 0),
                    Material = new DiffuseMaterial
                    {
                        DiffuseMap = SunTexture
                    }
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.3075, 0, 0), 3.8)
                {
                    BodyName = "Merkury",
                    Mass = AstronomicalObject.Me * 0.0552,
                    Velocity =
                        new Vector3D(0, -58.98e3 * Math.Sin(ToRadians(7.0)), -58.98e3 * Math.Cos(ToRadians(7.0))),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.71843270, 0, 0), 9.5)
                {
                    BodyName = "Wenus",
                    Mass = AstronomicalObject.Me * 0.8149,
                    Velocity = new Vector3D(0, -35.259e3 * Math.Sin(ToRadians(3.4)),
                        -35.259e3 * Math.Cos(ToRadians(3.4))),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 0.98329134, 0, 0), 10)
                {
                    BodyName = "Ziemia",
                    Mass = AstronomicalObject.Me,
                    Velocity = new Vector3D(0, 0, -30.29e3),
                    Material = DiffuseMaterials.Blue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 1.3814, 0, 0), 5.3)
                {
                    BodyName = "Mars",
                    Mass = AstronomicalObject.Me * 0.1074,
                    Velocity =
                        new Vector3D(0, -26.5e3 * Math.Sin(ToRadians(1.9)), -26.5e3 * Math.Cos(ToRadians(1.9))),
                    Material = DiffuseMaterials.Copper
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 5.034, 0, 0), 112)
                {
                    BodyName = "Jowisz",
                    Mass = AstronomicalObject.Me * 317.83,
                    Velocity =
                        new Vector3D(0, -13.72e3 * Math.Sin(ToRadians(1.3)), -13.72e3 * Math.Cos(ToRadians(1.3))),
                    Material = DiffuseMaterials.BlanchedAlmond,
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 9.195, 0, 0), 5.3)
                {
                    BodyName = "Saturn",
                    Mass = AstronomicalObject.Me * 95.1620,
                    Velocity =
                        new Vector3D(0, -10.18e3 * Math.Sin(ToRadians(2.5)), -10.18e3 * Math.Cos(ToRadians(2.5))),
                    Material = DiffuseMaterials.Bisque
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 18.637, 0, 0), 39.81)
                {
                    BodyName = "Uran",
                    Mass = AstronomicalObject.Me * 14.536,
                    Velocity = new Vector3D(0, -7.11e3 * Math.Sin(ToRadians(0.8)), -7.11e3 * Math.Cos(ToRadians(0.8))),
                    Material = DiffuseMaterials.LightBlue
                },
                new AstronomicalObject(new Point3D(AstronomicalObject.Au * 30.216, 0, 0), 38.65)
                {
                    BodyName = "Neptun",
                    Mass = AstronomicalObject.Me * 17.1470,
                    Velocity = new Vector3D(0, -5.5e3 * Math.Sin(ToRadians(1.8)), -5.5e3 * Math.Cos(ToRadians(1.8))),
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