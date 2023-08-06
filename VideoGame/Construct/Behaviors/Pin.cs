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
    public class Pin : Behavior
    {

        private Sprite Board;
        private Vector2 Offset;
        private Vector2 PinPosition { get => Board is null? Vector2.Zero: Board.Position + Offset; }

        public override void Act(TimeSpan deltaTime)
        {
            if (Board.Disposed) Board = null;
            if (!(Board is null))
                Parent.Set(PinPosition);
        }

        public Pin(Sprite board, Vector2 offset, bool enabled)
        {
            Board = board;
            Offset = offset;
            Enabled = enabled;
        }
    }
}
