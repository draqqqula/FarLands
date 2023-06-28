using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// уровень игры
    /// </summary>
    public class Level
    {
        public bool OnPause { get; private set; }
        public void Pause()
        {
            OnPause = true;
        }
        public void Resume()
        {
            OnPause = false;
        }
        public void TogglePause()
        {
            OnPause = !OnPause;
        }
        /// <summary>
        /// текущее состояние уровня
        /// </summary>
        public IGameState GameState { get; set; }
        public readonly string Name;
        /// <summary>
        /// функция, возвращающая новое состояние уровня
        /// </summary>
        public readonly Func<World, ContentManager, string, IGameState> Initialize;

        public Level(string name, Func<World, ContentManager, string, IGameState> initialize)
        {
            Name = name;
            Initialize = initialize;
            OnPause = false;
        }
    }
}
