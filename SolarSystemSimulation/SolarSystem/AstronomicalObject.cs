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

        public double Mass { get; set; }

        private Vector3D _gravity;

        public Vector3D PrevVelocityDot { get; private set; }
        public Vector3D PrevVelocityDot2 { get; private set; }
        public Vector3D Velocity { get; set; }

        public Vector3D PrevPositionDot { get; private set; }
        public Vector3D PrevPositionDot2 { get; private set; }
        public Point3D Position { get; set; }

        private int _step = 1;

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
            Vector3D tempPos;
            Vector3D tempVel;

            switch (_step)
            {
                case 1:
                    _step++;

                    PrevPositionDot = Velocity * dt;
                    PrevVelocityDot = _gravity * dt;

                    Position += PrevPositionDot;
                    Velocity += PrevVelocityDot;
                    break;
                case 2:
                    _step++;

                    tempPos = Velocity * dt;
                    tempVel = _gravity * dt;

                    Position += tempPos * 1.5 - PrevPositionDot * 0.5;
                    Velocity += tempVel * 1.5 - PrevVelocityDot * 0.5;

                    PrevPositionDot = tempPos;
                    PrevVelocityDot = tempVel;
                    break;
                default:
                    tempPos = Velocity * dt;
                    tempVel = _gravity * dt;

                    Position += tempPos * 23f / 12f - PrevPositionDot * 16f / 12f + PrevPositionDot2 * 5f / 12f;
                    Velocity += tempVel * 23f / 12f - PrevVelocityDot * 16f / 12f + PrevVelocityDot2 * 5f / 12f;

                    PrevPositionDot2 = PrevPositionDot;
                    PrevVelocityDot2 = PrevVelocityDot;

                    PrevPositionDot = tempPos;
                    PrevVelocityDot = tempVel;
                    break;
            }

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