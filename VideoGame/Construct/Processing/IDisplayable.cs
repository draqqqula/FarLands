using Microsoft.Xna.Framework.Graphics;

namespace VideoGame
{
    public interface IDisplayable
    {
        public void Draw(SpriteBatch batch, GameCamera camera, ContentStorage contentStorage);
    }
}
