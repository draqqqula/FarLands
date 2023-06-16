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

        public Layer MainLayer;
        public Layer FrontParticlesLayer;
        public Layer BackParticlesLayer;

        public List<GameObject> AllObjects { get; set; }

        public GameObject Player;

        public TextObject FPSCounter;

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
            var dummy = Player.GetBehavior<Dummy>("Dummy");
            if (dummy.Health == 0)
                Global.Variables.MainGame._world.GoNext(Global.Variables.MainGame._world.CurrentLevel.Name);
            FPSCounter.Text = Convert.ToString(Math.Round(1/Global.Variables.DeltaTime.TotalSeconds));
        }

        public LocationState()
        {
            Layers = new Dictionary<string, Layer>();
            AllObjects = new List<GameObject>();
            Patterns = new List<IPattern>();
        }
    }
}
