using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Описывает поведение объекта, который может колебаться в определённом направлении
    /// </summary>
    public class Sine : IBehavior
    {
        public double t { get; private set; }
        public double Amplitude { get; private set; }
        public double Value { get { return Amplitude * Math.Sin(Factor * t); } }
        public Vector2 Direction { get; private set; }
        public double Factor { get; private set; }
        public string Name => "Sine";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void ChangeDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public void Act()
        {
            t += Global.Variables.DeltaTime.TotalSeconds;
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            parameters.Position += (float)Value * Direction;
            return parameters;
        }

        public Sine(double t0, double amplitude, Vector2 direction, double factor, bool enabled)
        {
            t = t0;
            Amplitude = amplitude;
            Direction = direction;
            Factor = factor;
            Enabled = enabled;
        }
    }
}
