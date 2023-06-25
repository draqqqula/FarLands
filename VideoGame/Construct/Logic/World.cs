using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nito.Collections;
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
        public bool IsReady
        {
            get => CurrentLevels.Count > 0;
        }

        private readonly Dictionary<string, Level> Levels;
        /// <summary>
        /// текущие уровни, которые будут обновляться и отрисовываться
        /// </summary>
        private Dictionary<string,Level> CurrentLevels { get; set; }
        /// <summary>
        /// загружает уровень, инициализируя новое состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void LoadLevel(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            if (!CurrentLevels.ContainsKey(levelName))
            {
                CurrentLevels.Add(levelName, level);
            }
            level.GameState = level.Initialize(this, content, levelName);
        }
        /// <summary>
        /// загружает уровень, используя существующее состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void GoNext(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            CurrentLevels.Add(levelName, level);
            if (level.GameState == null) level.Initialize(this, content, levelName);
        }

        public void Pass(string from, string to, ContentManager content)
        {
            Levels.Remove(from);
            LoadLevel(to, content);
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
            if (IsReady)
            {
                foreach(var level in CurrentLevels.Values.ToArray())
                    level.GameState.Update(deltaTime, clientBounds);
            }
        }

        public World()
        {
            Levels = new Dictionary<string, Level>();
            CurrentLevels = new Dictionary<string, Level>();
        }

        public void Display(SpriteBatch spriteBatch)
        {
            foreach(var level in CurrentLevels.Values)
            {
                DisplayLevel(spriteBatch, level);
            }
        }

        private void DisplayLevel(SpriteBatch spriteBatch, Level level)
        {
            foreach (var layer in level.GameState.Layers.Values)
            {
                foreach (var drawable in layer.DrawBuffer.Values)
                    drawable.Draw(spriteBatch);
                foreach (var tileMap in layer.TileMaps)
                    tileMap.Draw(spriteBatch, level.GameState.Camera);
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
        public readonly Func<World,ContentManager,string,IGameState> Initialize;

        public Level(string name, Func<World,ContentManager,string,IGameState> initialize)
        {
            Name = name;
            Initialize = initialize;
        }
    }
}
