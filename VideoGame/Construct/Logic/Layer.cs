using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Microsoft.Xna.Framework;

namespace VideoGame
{
    public class Layer
    {
        public readonly string Name;
        public Func<Vector2, Vector2> DrawingFunction;
        public readonly double DrawingPriority;

        public Dictionary<Animator, DrawableElement> DrawBuffer;
        public List<TileMap> TileMaps;
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
