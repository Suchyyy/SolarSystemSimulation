using System;
using System.Collections.Generic;
using System.Linq;
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

        public string BodyName { get; set; }

        private readonly TranslateTransform3D _translateTransform3D;

        public double Mass { get; set; }

        private Vector3D _gravity;

        private readonly Vector3D[] _velocityDots = Enumerable.Range(0, 3).Select(i => new Vector3D(0, 0, 0)).ToArray();
        public Vector3D Velocity { get; set; }

        private readonly Vector3D[] _positionDots = Enumerable.Range(0, 3).Select(i => new Vector3D(0, 0, 0)).ToArray();
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
            // @formatter:off
            switch (_step)
            {
                case 1: Step1(dt); break;
                case 2: Step2(dt); break;
                case 3: Step3(dt); break;
                default: Step4(dt); break;
            }
            // @formatter:on

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

        private void Step1(double dt)
        {
            _step++;

            _positionDots[0] = Velocity * dt;
            _velocityDots[0] = _gravity * dt;

            Position += _positionDots[0];
            Velocity += _velocityDots[0];
        }

        private void Step2(double dt)
        {
            _step++;

            var tempPos = Velocity * dt;
            var tempVel = _gravity * dt;

            Position += tempPos * 1.5f - _positionDots[0] * 0.5f;
            Velocity += tempVel * 1.5f - _velocityDots[0] * 0.5f;

            _positionDots[0] = tempPos;
            _velocityDots[0] = tempVel;
        }

        private void Step3(double dt)
        {
            _step++;

            const float p1 = 23f / 12f;
            const float p2 = 16f / 12f;
            const float p3 = 5f / 12f;

            var tempPos = Velocity * dt;
            var tempVel = _gravity * dt;

            Position += tempPos * p1 - _positionDots[0] * p2 + _positionDots[1] * p3;
            Velocity += tempVel * p1 - _velocityDots[0] * p2 + _velocityDots[1] * p3;

            _positionDots[1] = _positionDots[0];
            _velocityDots[1] = _velocityDots[0];

            _positionDots[0] = tempPos;
            _velocityDots[0] = tempVel;
        }

        private void Step4(double dt)
        {
            const float p1 = 54f / 24f;
            const float p2 = 59f / 24f;
            const float p3 = 37f / 24f;
            const float p4 = 9f / 24f;

            var tempPos = Velocity * dt;
            var tempVel = _gravity * dt;

            Position += tempPos * p1
                        - _positionDots[0] * p2
                        + _positionDots[1] * p3
                        - _positionDots[2] * p4;
            Velocity += tempVel * p1
                        - _velocityDots[0] * p2
                        + _velocityDots[1] * p3
                        - _velocityDots[2] * p4;

            _positionDots[2] = _positionDots[1];
            _velocityDots[2] = _velocityDots[1];

            _positionDots[1] = _positionDots[0];
            _velocityDots[1] = _velocityDots[0];

            _positionDots[0] = tempPos;
            _velocityDots[0] = tempVel;
        }
    }
}