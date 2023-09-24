using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        public readonly ContentStorage contentStorge;
        public bool IsReady
        {
            get => CurrentLevels.Length > 0;
        }

        public bool IsLevelActive(Level level)
        {
            return CurrentLevels.Contains(level);
        }
        public bool IsLevelActive(string levelName)
        {
            return CurrentLevels.Contains(Levels[levelName]);
        }

        private readonly Dictionary<string, Level> Levels = new Dictionary<string, Level>();
        /// <summary>
        /// текущие уровни, которые будут обновляться и отрисовываться
        /// </summary>
        private RelativeCollection<Level> CurrentLevels { get; set; } = new RelativeCollection<Level>();

        /// <summary>
        /// загружает уровень, инициализируя новое состояние
        /// </summary>
        /// <param name="levelName"></param>
        public Level LoadRootLevel(string levelName, ContentStorage content)
        {
            var level = Load(levelName, content);
            if (!IsLevelActive(levelName))
            {
                CurrentLevels.PlaceTop(level);
            }
            return level;
        }

        public Level Load(string levelName, ContentStorage content)
        {
            var level = Levels[levelName];
            level.GameState = level.Initialize(this, content, levelName);
            return level;
        }
        public void UnloadLevel(string levelName)
        {
            CurrentLevels.Remove(Levels[levelName]);
        }
        /// <summary>
        /// загружает уровень, используя существующее состояние
        /// </summary>
        /// <param name="levelName"></param>
        public void GoNext(string levelName, ContentManager content)
        {
            var level = Levels[levelName];
            CurrentLevels.PlaceTop(level);
            if (level.GameState == null) level.Initialize(this, content, levelName);
        }

        public void Pass(string from, string to, ContentManager content)
        {
            UnloadLevel(from);
            LoadRootLevel(to, content);
        }

        /// <summary>
        /// добавляет уровень
        /// </summary>
        /// <param name="level"></param>
        public void AddLevel(Level level)
        {
            Levels.Add(level.Name, level);
        }

        public void Update(TimeSpan deltaTime, GameWindow window)
        {
            Client.Window = window.ClientBounds;
            if (IsReady)
            {
                foreach(var level in CurrentLevels.ToArray())
                    level.GameState.Update(deltaTime, level.OnPause);
            }
        }

        public World(GameClient client, ContentStorage contentStorage)
        {
            Client = client;
            contentStorge = contentStorage;
        }

        public void Display(SpriteBatch spriteBatch)
        {
            DisplayBranch(CurrentLevels, spriteBatch);
        }

        private void DisplayBranch(IEnumerable<Level> states, SpriteBatch batch)
        {
            foreach (var level in states)
            {
                DisplayLevel(batch, level);
                DisplayBranch(level.GameState.LevelLoader, batch);
            }
        }

        private void DisplayLevel(SpriteBatch spriteBatch, Level level)
        {
            if (level.GameState.IsConnected(Client))
            {
                var camera = level.GameState.GetCamera(Client);
                foreach (var layer in level.GameState.Layers.Values)
                {
                    var view = layer.PointsOfView[camera];

                    foreach (var drawable in view.Pictures)
                    {
                        drawable.Draw(spriteBatch, camera, contentStorge);
                    }
                }
            }
        }

        public void PauseLevel(string name)
        {
            Levels[name].Pause();
        }

        public void ResumeLevel(string name)
        {
            Levels[name].Resume();
        }
    }
}
