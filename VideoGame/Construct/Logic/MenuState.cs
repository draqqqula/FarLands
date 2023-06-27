using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class MenuState : IGameState
    {
        public AnimationBuilder MainAnimationBuilder { get; set; }
        public LevelLoader LevelLoader { get; set; }
        public Dictionary<string, Layer> Layers { get; set; }
        public List<GameObject> AllObjects { get; set; }
        public List<IPattern> Patterns { get; set; }
        public GameControls Controls { get; set; }
        public GameCamera Camera { get; set; }

        public void LocalUpdate(TimeSpan deltaTime)
        {
            if (Controls.OnPress(Control.pause))
            {
                LevelLoader.Unload();
            }
        }

        public MenuState()
        {
            Layers = new Dictionary<string, Layer>();
            AllObjects = new List<GameObject>();
            Patterns = new List<IPattern>();
        }
    }
}
