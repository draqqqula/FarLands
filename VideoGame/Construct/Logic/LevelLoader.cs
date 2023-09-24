using Microsoft.Xna.Framework.Content;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class LevelLoader
    {
        private Level ThisLevel;
        private World World;
        private ContentManager Content;

        public bool IsReadyToResume
        {
            get => true;
        }
        public Level LoadLevel(string name)
        {
            return World.LoadRootLevel(name);
        }

        public void Pass(string name)
        {
            World.Pass(ThisLevel, name, Content);
        }
        public void RestartLevel()
        {
            World.LoadRootLevel(ThisLevel, Content);
        }

        public void Pause(Level pauseHandler)
        {
            World.PauseLevel(ThisLevel);
        }

        public void Resume()
        {
            World.ResumeLevel(ThisLevel);
        }

        public void Unload()
        {
            World.UnloadLevel(ThisLevel);
        }

        public LevelLoader(World world, ContentManager content, Level thisLevel)
        {
            World = world;
            Content = content;
            ThisLevel = thisLevel;
        }
    }
}
