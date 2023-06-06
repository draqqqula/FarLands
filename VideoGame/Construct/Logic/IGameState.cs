using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public interface IGameState
    {
        public void Update()
        {
            Camera.Update();
            ExcludeDestroyed();
            UpdateBehaviors();
            UpdateAnimations();
            UpdatePatterns();
            LocalUpdate();
        }

        public void LocalUpdate();

        public Dictionary<string, Layer> Layers { get; set; }
        public List<GameObject> AllObjects { get; set; }
        public List<IPattern> Patterns { get; set; }
        public GameControls Controls { get; set; }
        public GameCamera Camera { get; set; }

        public void UpdateAnimations()
        {
            foreach (var sprite in AllObjects)
            {
                sprite.UpdateAnimation();
            }
        }

        public void UpdateBehaviors()
        {
            foreach (var sprite in AllObjects)
            {
                foreach (var behavior in sprite.ActiveBehaviors.Values)
                {
                    behavior.Act();
                }
            }
        }

        public void UpdatePatterns()
        {
            foreach (var pattern in Patterns)
            {
                pattern.Update(this);
            }
        }

        public void ExcludeDestroyed()
        {
            AllObjects.RemoveAll(e => e.ToDestroy);
            Patterns.ForEach(pattern => pattern.Editions.RemoveAll(edition => edition.ToDestroy));
        }
    }
}
