using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame.Construct.Behaviors
{
    public class Collider : IBehavior
    {
        private Vector2 LastKnownPosition;
        public string Name => "Collider";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void Act()
        {
            LastKnownPosition = Parent.Position;
        }

        public IEnumerable<Collider> GetCollisions(IEnumerable<Collider> colliders)
        {
            foreach (var collider in colliders)
            {
                if (collider.Parent.Layout.Intersects(Parent.Layout))
                    yield return collider;
            }
        }
        public IEnumerable<Collider> GetCollisions(params Collider[] colliders)
        {
            return GetCollisions((IEnumerable<Collider>)colliders);
        }

        public bool Collides(Collider collider)
        {
            return collider.Parent.Layout.Intersects(Parent.Layout);
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }
    }
}
