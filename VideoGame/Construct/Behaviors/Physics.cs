using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Microsoft.Xna.Framework;

namespace VideoGame
{
    public enum Side
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class MovementVector
    {
        public Vector2 Vector
        {
            get
            {
                return Direction * Module;
            }
        }
        public float Module;
        public Vector2 Direction;
        public float Acceleration;
        public bool Enabled;

        public TimeSpan LifeTime;
        public TimeSpan LivingTime;
        public bool Immortal;

        public bool Update()
        {
            if (Enabled)
            {
                if (Acceleration != 0)
                {
                    Module += Acceleration * (float)Global.Variables.DeltaTime.TotalSeconds;
                    if (Module <= 0)
                    {
                        return false;
                    }
                }

                if (!Immortal)
                {
                    LifeTime += Global.Variables.DeltaTime;
                    if (LifeTime >= LivingTime)
                    {
                        return false;
                    }
                }

            }
            return true;
        }

        public MovementVector(Vector2 vector, float acceleration, TimeSpan livingTime, bool immortal) :
            this(
                MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y),
                vector / MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y),
                acceleration,
                true,
                livingTime,
                immortal
                )
        {
        }

        public MovementVector(float module, Vector2 direction, float acceleration, bool enabled, TimeSpan livingTime, bool immortal)
        {
            Module = module;
            Direction = direction;
            Acceleration = acceleration;
            Enabled = enabled;
            LivingTime = livingTime;
            Immortal = immortal;
        }
    }

    public class Physics : IBehavior
    {
        public readonly Rectangle[][] SurfaceMap;
        public readonly int SurfaceWidth;
        public Dictionary<Side, bool> Faces;
        public Dictionary<string, MovementVector> Vectors { get; private set; }

        public void AddVector(string name, MovementVector vector)
        {
            Vectors[name] = vector;
        }

        public void RemoveVector(string name)
        {
            Vectors.Remove(name);
        }

        public IEnumerable<Rectangle> GetMapSegment(int start, int end)
        {
            var imaginaryStart = Math.Min(Math.Max(start / SurfaceWidth, 0), SurfaceMap.Length);
            var imaginaryEnd = Math.Max(0, Math.Min(end / SurfaceWidth + 1, SurfaceMap.Length));
            return Enumerable
                .Range(imaginaryStart, imaginaryEnd - imaginaryStart)
                .Select(n => SurfaceMap[n])
                .SelectMany(e => e);
        }

        private Vector2 ApplyCollision(Rectangle start, IEnumerable<Rectangle> surfaces, Rectangle end)
        {
            foreach (Side side in (Side[])Enum.GetValues(typeof(Side)))
                Faces[side] = false;
            var moving = new Rectangle(end.Location, end.Size);



            var surfaceList = surfaces.ToList();
            while (surfaceList.Count > 0)
            {
                var surface = surfaceList.MaxBy(r => { var i = Rectangle.Intersect(moving, r); return i.Width * i.Height; });

                var intersection = Rectangle.Intersect(moving, surface);
                if (!intersection.IsEmpty)
                {
                    var distance = moving.Center - intersection.Center;
                    if (intersection.Width > intersection.Height)
                    {
                        var factor = distance.Y / Math.Abs(distance.Y);

                        if (factor > 0) Faces[Side.Top] = true;
                        else Faces[Side.Bottom] = true;

                        moving.Offset(0, intersection.Height * factor);
                    }
                    else if (intersection.Width < intersection.Height)
                    {
                        var factor = distance.X / Math.Abs(distance.X);

                        if (factor > 0) Faces[Side.Right] = true;
                        else Faces[Side.Left] = true;

                        moving.Offset(intersection.Width * factor, 0);
                    }
                }
                surfaceList.Remove(surface);
            }
            return (moving.Location - end.Location).ToVector2();
        }

        public string Name { get => "Physics"; }
        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void Act()
        {
            Vector2 resultingVector = Vector2.Zero;
            foreach (var vector in Vectors)
            {
                resultingVector += vector.Value.Vector * (float)(Global.Variables.DeltaTime.TotalSeconds * 60);
                if (!vector.Value.Update())
                    Vectors.Remove(vector.Key);
            }
            var resultingLength = resultingVector.Length();
            var direction = Vector2.Normalize(resultingVector);
            var allowedSpeed = SurfaceWidth/4;

            for (int i = 0; i < Math.Ceiling(resultingVector.Length() / allowedSpeed); i++)
            {
                var sequenceLength = Math.Min(allowedSpeed, resultingLength - i * allowedSpeed);
                var vectorSequence = direction * sequenceLength;

                var pastPosition = Parent.Layout;
                var futurePosition = Parent.PredictLayout(vectorSequence);
                var mapSegment = GetMapSegment(Math.Min(pastPosition.Left, futurePosition.Left), Math.Max(pastPosition.Right, futurePosition.Right));
                var collisionFactor = ApplyCollision(pastPosition, mapSegment, futurePosition);
                Parent.Position += vectorSequence + collisionFactor;
            }
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public Physics(Rectangle[][] surfaceMap, int surfaceWidth, bool enabled)
        {
            SurfaceMap = surfaceMap;
            SurfaceWidth = surfaceWidth;
            Vectors = new Dictionary<string, MovementVector>();
            Enabled = enabled;

            Faces = new Dictionary<Side, bool>();
            foreach (Side side in (Side[])Enum.GetValues(typeof(Side)))
                Faces[side] = false;
        }
    }
}
