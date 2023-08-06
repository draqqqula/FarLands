using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// поведение, которое реализует объект
    /// </summary>
    public abstract class Behavior
    {
        /// <summary>
        /// объект, реализующий это поведение
        /// </summary>
        public Sprite Parent { get; set; }
        public bool Enabled { get; set; }
        /// <summary>
        /// вносит изменения в графическое представление объекта
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public virtual DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }
        /// <summary>
        /// вносит изменения в физическое представление объекта
        /// </summary>
        public virtual void Act(TimeSpan deltaTime)
        {
        }
    }
}
