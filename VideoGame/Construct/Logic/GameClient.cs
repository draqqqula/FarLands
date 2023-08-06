using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Globalization;

namespace VideoGame
{
    public class GameClient
    {
        public enum GameLanguage
        {
            Russian,
            English
        }

        public readonly GameLanguage Language;
        public readonly GameControls Controls;
        public readonly GameWindow Window;

        public GameClient(GameWindow window, GameControls controls, GameLanguage language)
        {
            Window = window;
            Controls = controls;
            Language = language;
        }
    }
}