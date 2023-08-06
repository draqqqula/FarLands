using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Families;

namespace VideoGame.Construct.Patterns
{
    [Box(22,120)]
    [SpriteSheet("Element_Selector")]
    [ReversedVisibility]
    public class ElementSelector : Sprite
    {
        public ElementSelector(GameState state, Vector2 position, Layer layer, GameClient viewer) :
        base(state, position, layer, false)
        {
            AddClient(viewer);
            OnAssembled(this);
        }
    }

    [Box(0,0,28,30)]
    [SpriteSheet("dash_bar")]
    [ReversedVisibility]
    public class DashBar : Sprite
    {
        public DashBar(GameState state, Vector2 position, Layer layer, GameClient viewer) :
            base (state, position, layer, false)
        {
            AddClient(viewer);
            PlaceAbove(new ElementSelector(state, new Vector2(136, 85), layer, viewer));
            OnAssembled (this);
        }
    }
}
