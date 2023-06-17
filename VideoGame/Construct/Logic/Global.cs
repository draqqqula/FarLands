using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame;
using Animations;


//подлежит удалению
namespace Global
{
    public static class Properties
    {
        public static bool DrawHitBox = false;
    }
    public static class Variables
    {
        public static SpriteBatch MainSpriteBatch;
        public static Microsoft.Xna.Framework.Content.ContentManager MainContent
        {
            get
            {
                return MainGame.Content;
            }
        }
        public static TimeSpan DeltaTime;
        public static Game1 MainGame;
    }
}
