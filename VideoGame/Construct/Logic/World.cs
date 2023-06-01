using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame.Construct
{
    public class World
    {
        private readonly Dictionary<string, Level> Levels;
        public Level CurrentLevel { get; private set; }
        public void LoadLevel(string name)
        {
            var level = Levels[name];
            CurrentLevel = level;
            level.GameState = level.Initialize();
        }

        public void GoNext(string levelName)
        {
            LoadLevel(levelName);
        }

        public void AddLevel(Level level)
        {
            Levels.Add(level.Name, level);
        }

        public void Update()
        {
            if (CurrentLevel != null)
                CurrentLevel.GameState.Update();
        }

        public World()
        {
            Levels = new Dictionary<string, Level>();
        }
    }

    public class Level
    {
        public IGameState GameState { get; set; }
        public readonly string Name;
        public readonly Func<IGameState> Initialize;
        public readonly UnitAction Deconstruct;

        public Level(string name, Func<IGameState> initialize)
        {
            Name = name;
            Initialize = initialize;
        }
    }
}
