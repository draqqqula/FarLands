using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class LevelLoader
    {
        private Level PauseHandler;
        private string ThisLevel;
        private World World;
        private ContentManager Content;

        public bool IsReadyToResume
        {
            get => PauseHandler is null || !World.IsLevelActive(PauseHandler);
        }
        public Level LoadLevel(string name)
        {
            return World.LoadLevel(name, Content);
        }
        public void Pass(string name)
        {
            World.Pass(ThisLevel, name, Content);
        }
        public void RestartLevel()
        {
            World.LoadLevel(ThisLevel, Content);
        }

        public void Pause(Level pauseHandler)
        {
            World.PauseLevel(ThisLevel);
            PauseHandler = pauseHandler;
        }

        public void Resume()
        {
            World.ResumeLevel(ThisLevel);
        }

        public void Unload()
        {
            World.UnloadLevel(ThisLevel);
        }

        public LevelLoader(World world, ContentManager content, string thisLevel)
        {
            World = world;
            Content = content;
            ThisLevel = thisLevel;
        }
    }
}
