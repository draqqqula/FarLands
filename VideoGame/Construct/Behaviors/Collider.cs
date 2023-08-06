using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinRT;

namespace VideoGame.Construct.Behaviors
{
    /// <summary>
    /// позволяет обрабатывать коллизии между объектами
    /// </summary>
    public class Collider : Behavior
    {
        private Vector2 LastKnownPosition;
        public IEnumerable<Rectangle> Path;
        public int Accuracy;

        public override void Act(TimeSpan deltaTime)
        {
            if (LastKnownPosition == null)
            {
                Vector2 movement = Parent.Position - LastKnownPosition;
                float length = movement.Length();
                List<Rectangle> path = new List<Rectangle>();
                movement.Normalize();
                for (int l = 0; l <= length; l += Accuracy)
                {
                    path.Add(Parent.PredictLayout(l * movement));
                }
                path.Add(Parent.Layout);
                Path = path;
            }
            else
            {
                Path = new Rectangle[] { Parent.Layout };
            }
            LastKnownPosition = Parent.Position;
        }

        public IEnumerable<Collider> GetCollisions(IEnumerable<Collider> colliders)
        {
            return colliders.Where(c => c.Collides(this));
        }
        public IEnumerable<Collider> GetCollisions(params Collider[] colliders)
        {
            return GetCollisions((IEnumerable<Collider>)colliders);
        }

        public bool Collides(Collider collider)
        {
            if (collider.Path is null || this.Path is null)
                return false;
            return collider.Path.Any(r1 => Path.Any(r2 => r1.Intersects(r2)));
        }

        public Collider(int accuracy, bool enabled)
        {
            Accuracy = accuracy;
            Enabled = enabled;
        }
    }
}
