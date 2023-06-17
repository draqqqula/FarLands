using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Описывает поведение объекта, "приклеенного" к другому объекту
    /// </summary>
    public class Pin : IBehavior
    {
        public string Name => "Pin";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        private GameObject Board;
        private Vector2 Offset;
        private Vector2 PinPosition { get => Board is null? Vector2.Zero: Board.Position + Offset; }

        public void Act()
        {
            if (Board.ToDestroy) Board = null;
            if (!(Board is null))
                Parent.Position = PinPosition;
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public Pin(GameObject board, Vector2 offset, bool enabled)
        {
            Board = board;
            Offset = offset;
            Enabled = enabled;
        }
    }
}
