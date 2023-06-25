using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        public bool IsReadyToDisplay
        {
            get => CurrentLevel != null;
        }
        private IEnumerable<Layer> Layers
        {
            get => CurrentLevel.GameState.Layers.Values;
        }

        public void RestartLevel(ContentManager content)
        {
            LoadLevel(CurrentLevel.Name, content);
        }
        private readonly Dictionary<string, Level> Levels;
        /// <summary>
        /// текущий уровень
        /// </summary>
        private Level CurrentLevel { get; set; }
        /// <summary>
        /// загружает уровень, инициализируя новое состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            CurrentLevel = level;
            level.GameState = level.Initialize(this, content);
        }
        /// <summary>
        /// загружает уровень, используя существующее состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void GoNext(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            CurrentLevel = level;
            if (level.GameState == null) level.Initialize(this, content);
        }

        /// <summary>
        /// добавляет уровень
        /// </summary>
        /// <param name="level"></param>
        public void AddLevel(Level level)
        {
            Levels.Add(level.Name, level);
        }

        public void Update(TimeSpan deltaTime, Rectangle clientBounds)
        {
            if (CurrentLevel != null)
                CurrentLevel.GameState.Update(deltaTime, clientBounds);
        }

        public World()
        {
            Levels = new Dictionary<string, Level>();
        }

        public void Display(SpriteBatch spriteBatch)
        {
            foreach (var layer in Layers)
            {
                foreach (var drawable in layer.DrawBuffer.Values)
                    drawable.Draw(spriteBatch);
                foreach (var tileMap in layer.TileMaps)
                    tileMap.Draw(spriteBatch, CurrentLevel.GameState.Camera);
                foreach (var text in layer.TextObjects)
                    text.Draw(spriteBatch);
                layer.DrawBuffer.Clear();
            }
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
        public readonly Func<World,ContentManager,IGameState> Initialize;

        public Level(string name, Func<World,ContentManager,IGameState> initialize)
        {
            Name = name;
            Initialize = initialize;
        }
    }
}
