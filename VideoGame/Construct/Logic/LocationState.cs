using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;

namespace VideoGame
{
    public class LocationState : IGameState
    {
        public Dictionary<string, Layer> Layers { get; set; }
        public List<GameObject> AllObjects { get; set; }

        public GameObject Player;

        public TextObject HealthBar;

        public GameControls Controls { get; set; }
        public GameCamera Camera { get; set; }
        public List<IPattern> Patterns { get; set; }

        public TileMap MainTileMap;

        public void AddLayers(params Layer[] layers)
        {
            foreach (var layer in layers)
                Layers.Add(layer.Name, layer);
            Layers = Layers.OrderBy(e => e.Value.DrawingPriority).ToDictionary(e => e.Key, e => e.Value);
        }

        public void AddPatterns(params IPattern[] families)
        {
            foreach (var family in families)
                Patterns.Add(family);
        }

        public void LocalUpdate()
        {
            StringBuilder healthText = new StringBuilder();
            var dummy = Player.GetBehavior<Dummy>("Dummy");
            for (int i = 0; i < dummy.MaxHealth; i++)
                healthText.Append(i < dummy.Health ?'a' : 'b');
            HealthBar.Text = healthText.ToString();
            if (dummy.Health == 0)
                Global.Variables.MainGame._world.GoNext(Global.Variables.MainGame._world.CurrentLevel.Name);
        }

        public LocationState()
        {
            Layers = new Dictionary<string, Layer>();
            AllObjects = new List<GameObject>();
            Patterns = new List<IPattern>();
        }
    }
}
