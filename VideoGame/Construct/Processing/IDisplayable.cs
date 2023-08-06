using Microsoft.Xna.Framework.Graphics;

namespace VideoGame
{
    public interface IDisplayable
    {
        public bool IsImmutable { get; }
        public void Draw(SpriteBatch batch, GameCamera camera, SpriteDrawer streamDrawer);
    }
}
