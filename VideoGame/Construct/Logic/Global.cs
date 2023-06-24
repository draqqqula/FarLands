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
    public static class Variables
    {
        public static Microsoft.Xna.Framework.Content.ContentManager MainContent
        {
            get
            {
                return MainGame.Content;
            }
        }
        public static Game1 MainGame { get; set; }
    }
}
