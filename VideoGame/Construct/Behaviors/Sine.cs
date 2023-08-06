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
    public class Sine : Behavior
    {
        public double t { get; private set; }
        public double Amplitude { get; private set; }
        public double Value { get { return Amplitude * Math.Sin(Factor * t); } }
        public Vector2 Direction { get; private set; }
        public double Factor { get; private set; }

        public void ChangeDirection(Vector2 direction)
        {
            Direction = direction;
        }

        public override void Act(TimeSpan deltaTime)
        {
            t += deltaTime.TotalSeconds;
        }

        public override DrawingParameters ChangeAppearance(DrawingParameters parameters)
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
