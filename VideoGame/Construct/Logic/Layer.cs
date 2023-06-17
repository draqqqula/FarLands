using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Microsoft.Xna.Framework;

namespace VideoGame
{
    /// <summary>
    /// слой, на котором могут распологаться объекты, карты тайлов и текст
    /// </summary>
    public class Layer
    {
        public readonly string Name;
        /// <summary>
        /// функция, преобразующая физическое представление объекта в графическое
        /// </summary>
        public Func<Vector2, Vector2> DrawingFunction;
        /// <summary>
        /// Приоритет отрисовки относительно остальных слоёв.
        /// Слои с меньшим приоритетом отрисовываются первее.
        /// </summary>
        public readonly double DrawingPriority;

        /// <summary>
        /// буффер, в который помещаются кадры, подлежащие отрисовке
        /// </summary>
        public Dictionary<Animator, DrawableElement> DrawBuffer;
        /// <summary>
        /// карты тайлов, принадлежащие этому слою
        /// </summary>
        public List<TileMap> TileMaps;
        /// <summary>
        /// текстовые объекты, принадлежащие этому слою
        /// </summary>
        public List<TextObject> TextObjects;
        public Layer(string name, Func<Vector2, Vector2> drawingFunction, double drawingPriority)
        {
            DrawingFunction = drawingFunction;
            DrawingPriority = drawingPriority;
            DrawBuffer = new Dictionary<Animator, DrawableElement>();
            TileMaps = new List<TileMap>();
            TextObjects = new List<TextObject>();
            Name = name;
        }
    }
}
