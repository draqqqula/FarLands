using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame.Construct
{
    /// <summary>
    /// Содержит все уровни.
    /// Позволяет переключаться между разными уровнями игры.
    /// </summary>
    public class World
    {
        private readonly Dictionary<string, Level> Levels;
        /// <summary>
        /// текущий уровень
        /// </summary>
        public Level CurrentLevel { get; private set; }
        /// <summary>
        /// загружает уровень, инициализируя новое состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName)
        {
            var level = Levels[levelName];
            CurrentLevel = level;
            level.GameState = level.Initialize();
        }
        /// <summary>
        /// загружает уровень, используя существующее состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void GoNext(string levelName)
        {
            var level = Levels[levelName];
            CurrentLevel = level;
            if (level.GameState == null) level.Initialize();
        }

        /// <summary>
        /// добавляет уровень
        /// </summary>
        /// <param name="level"></param>
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

    /// <summary>
    /// уровень игры
    /// </summary>
    public class Level
    {
        /// <summary>
        /// текущее состояние уровня
        /// </summary>
        public IGameState GameState { get; set; }
        public readonly string Name;
        /// <summary>
        /// функция, возвращающая новое состояние уровня
        /// </summary>
        public readonly Func<IGameState> Initialize;

        public Level(string name, Func<IGameState> initialize)
        {
            Name = name;
            Initialize = initialize;
        }
    }
}
