using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    [NoVisuals]
    public class Gate : Sprite
    {
        private string DestinationLevelName;
        private List<Character> Players;
        public Gate(GameState state, Vector2 position, Rectangle hitBox, Layer layer, string destinationLevelName, List<Character> players)
            : base(state, position, layer, false)
        {
            DestinationLevelName = destinationLevelName;
            Players = players;
        }

        public override void OnTick(GameState state, TimeSpan deltaTime)
        {
            if (Players.Any(it => it.Layout.Intersects(Layout)))
                state.LevelLoader.Pass(DestinationLevelName);
        }
    }
}
