using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame;
using Animations;

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

    public static class Updates
    {
        public static void UpdateAnimations()
        {
            foreach (var sprite in Containers.AllObjects)
            {
                sprite.UpdateAnimation();
            }
        }

        public static void UpdateBehaviors()
        {
            foreach (var sprite in Containers.AllObjects)
            {
                foreach(var behavior in sprite.Behaviors.Values)
                {
                   behavior.Act();
                }
            }
        }
    }

    public static class Containers
    {
        public static List<GameObject> AllObjects
        {
            get
            {
                return Global.Variables.MainGame._world.CurrentLevel.GameState.AllObjects;
            }
        }
    }
}
