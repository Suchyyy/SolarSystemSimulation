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
        public const double G = 6.6743e-11;
        public const double Au = 1.495e11;
        public const double Me = 5.9722e24;

        private readonly TranslateTransform3D _translateTransform3D;

        private Vector3D _gravity;
        public double Mass { get; set; }
        public Vector3D Velocity { get; set; }
        public Point3D Position { get; set; }
        public bool VisibleOrbits { get; set; } = true;

        public AstronomicalObject(Point3D position, double radius)
        {
            Position = position;

            var meshBuilder = new MeshBuilder(true, true, true);
            meshBuilder.AddSphere(new Vector3(0, 0, 0), radius, 64, 64);

            Geometry = meshBuilder.ToMesh();
            CullMode = CullMode.Back;

            _gravity = new Vector3D();

            _translateTransform3D = new TranslateTransform3D();
            Dispatcher?.Invoke(() =>
            {
                _translateTransform3D.OffsetX = Position.X / Au * 1e3;
                _translateTransform3D.OffsetY = Position.Y / Au * 1e3;
                _translateTransform3D.OffsetZ = Position.Z / Au * 1e3;
            });

            Transform = _translateTransform3D;
        }

        public void CalculateGravity(IEnumerable<AstronomicalObject> objects)
        {
            _gravity.X = 0;
            _gravity.Y = 0;
            _gravity.Z = 0;

            foreach (var body in objects)
            {
                if (body.Equals(this)) continue;

                var direction = body.Position - Position;
                var f = G * body.Mass / Math.Pow(direction.Length, 2);

                direction.Normalize();

                _gravity += direction * f;
            }
        }

        public void UpdatePosition(double dt)
        {
            Position += Velocity * dt;
            Velocity += _gravity * dt;

            try
            {
                Dispatcher?.Invoke(() =>
                {
                    _translateTransform3D.OffsetX = Position.X / Au * 1e3;
                    _translateTransform3D.OffsetY = Position.Y / Au * 1e3;
                    _translateTransform3D.OffsetZ = Position.Z / Au * 1e3;
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}