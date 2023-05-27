using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame.Construct.Behaviors
{
    public class FormulaPath : IBehavior
    {
        public Func<double,Vector2> Function { get; private set; }

        public double t;
        private Vector2 Start;

        public Vector2 Predict(TimeSpan dt)
        {
            return Predict(dt.TotalSeconds);
        }

        public Vector2 Predict(double dt)
        {
            return Function(t + dt);
        }

        public string Name => "FormulaPath";

        public GameObject Parent { get; set; }

        public void Act()
        {
            Parent.Position = Start + Function(t);
            t += Global.Variables.DeltaTime.TotalSeconds;
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public FormulaPath(Func<double, Vector2> function, double t0, Vector2 start)
        {
            Function = function;
            t = t0;
            Start = start;
        }
    }
}
