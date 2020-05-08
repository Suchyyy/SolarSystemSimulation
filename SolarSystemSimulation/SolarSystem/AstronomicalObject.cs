using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf.SharpDX;
using SharpDX;
using SharpDX.Direct3D11;

namespace SolarSystemSimulation.SolarSystem
{
    public class AstronomicalObject : MeshGeometryModel3D
    {
        public static readonly double G = 6.6743e-11;
        public static readonly double Au = 1.495e11;
        public static readonly double Me = 5.9722e24;

        public AstronomicalObject(double radius)
        {
            var meshBuilder = new MeshBuilder(true, true, true);
            meshBuilder.AddSphere(new Vector3(0, 0, 0), radius, 64, 64);

            Geometry = meshBuilder.ToMesh();
            CullMode = CullMode.Back;
        }

        public double Mass { get; set; }
        public Point3D Position { get; set; }

        public Vector3D Velocity { get; set; }

        public Vector3D GetGravity(IEnumerable<AstronomicalObject> objects)
        {
            var force = new Vector3D();

            foreach (var astronomicalObject in objects)
            {
                if (astronomicalObject.Equals(this)) continue;

                var direction = astronomicalObject.Position - Position;
                var f = G * Mass * astronomicalObject.Mass / Math.Pow(direction.Length, 2);

                direction.Normalize();

                force += direction * f;
            }

            return force;
        }

        public void UpdatePosition()
        {
            Position += Velocity;
        }
    }
}