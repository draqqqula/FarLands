using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;
using Windows.Foundation.Metadata;

namespace VideoGame
{
    [SpriteSheet("Biomass")]
    [Box(55, 55)]
    public class Biomass : Sprite
    {
        public Biomass(GameState state, Vector2 position, Layer layer, bool isMirrored) :
            base(state, position, layer, isMirrored)
        {
        }
    }
}
