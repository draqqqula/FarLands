using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace VideoGame
{
    /// <summary>
    /// Содержит все уровни.
    /// Позволяет переключаться между разными уровнями игры.
    /// </summary>
    public class World
    {
        public readonly GameClient Client;
        public readonly SpriteDrawer Drawer;
        public bool IsReady
        {
            get => CurrentLevels.Count > 0;
        }

        public bool IsLevelActive(Level level)
        {
            return CurrentLevels.ContainsValue(level);
        }
        public bool IsLevelActive(string levelName)
        {
            return CurrentLevels.ContainsKey(levelName);
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
        public Level LoadLevel(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            if (!CurrentLevels.ContainsKey(levelName))
            {
                CurrentLevels.Add(levelName, level);
            }
            level.GameState = level.Initialize(this, content, levelName);
            return level;
        }
        public void UnloadLevel(string levelName)
        {
            CurrentLevels.Remove(levelName);
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
            UnloadLevel(from);
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

        public void Update(TimeSpan deltaTime)
        {
            if (IsReady)
            {
                foreach(var level in CurrentLevels.Values.ToArray())
                    level.GameState.Update(deltaTime, level.OnPause);
            }
        }

        public World(GameClient client, SpriteDrawer drawer)
        {
            Client = client;
            Levels = new Dictionary<string, Level>();
            CurrentLevels = new Dictionary<string, Level>();
            Drawer = drawer;
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
            if (level.GameState.IsConnected(Client))
            {
                var camera = level.GameState.GetCamera(Client);
                Drawer.Draw(level.GameState.Layers.Values, camera, spriteBatch);
            }
        }

        public void PauseLevel(string name)
        {
            CurrentLevels[name].Pause();
        }

        public void ResumeLevel(string name)
        {
            CurrentLevels[name].Resume();
        }
    }
}
