using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Utilities;

namespace SolarSystemSimulation.SolarSystem
{
    public class SolarSystem
    {
        public List<AstronomicalObject> Bodies { get; set; }

        public SolarSystem()
        {
            Bodies = new List<AstronomicalObject>
            {
                new AstronomicalObject(109) /* sun */
                {
                    Mass = 333_000 * AstronomicalObject.Me,
                    Position = new Point3D(0, 0, 0),
                    Velocity = new Vector3D(0, 0, 0),
                    Material = DiffuseMaterials.Orange
                },
                new AstronomicalObject(3.8) /* mercury */
                {
                    Mass = AstronomicalObject.Me * 0.06,
                    Position = new Point3D(AstronomicalObject.Au * 0.39, 0, 0),
                    Velocity = new Vector3D(0, 0, -47.89e3),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(9.5) /* venus */
                {
                    Mass = AstronomicalObject.Me * 0.81,
                    Position = new Point3D(AstronomicalObject.Au * 0.72, 0, 0),
                    Velocity = new Vector3D(0, 0, -35e3),
                    Material = DiffuseMaterials.Yellow
                },
                new AstronomicalObject(10) /* earth */
                {
                    Mass = AstronomicalObject.Me,
                    Position = new Point3D(AstronomicalObject.Au, 0, 0),
                    Velocity = new Vector3D(0, 0, -30e3),
                    Material = DiffuseMaterials.Blue
                },
                new AstronomicalObject(5.3) /* mars */
                {
                    Mass = AstronomicalObject.Me * 0.11,
                    Position = new Point3D(AstronomicalObject.Au * 1.52, 0, 0),
                    Velocity = new Vector3D(0, 0, -24.13e3),
                    Material = DiffuseMaterials.Orange
                }
            };
        }

        public async void Update(int timestep)
        {
            while (true)
            {
                var task = Task.Delay(timestep);
                var day = 86_400.0 * timestep / 1000;

                Bodies.ForEach(body => body.CalculateGravity(Bodies));
                Bodies.ForEach(body => body.UpdatePosition(day * 182.5));

                await task;
            }
        }
    }
}