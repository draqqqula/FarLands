using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace VideoGame
{
    /// <summary>
    /// текстовый объект
    /// </summary>
    public class TextObject
    {
        public string Text { get; set; }

        public SpriteFont Font { get; set; }

        public Layer Layer { get; set; }

        public Vector2 Position { get; set; }

        public float Scale { get; set; }


        private DrawingParameters DrawingParameters
        {
            get
            {
                return new DrawingParameters
                {
                    Position = Layer.DrawingFunction(this.Position),
                    Layer = Layer,
                };
            }
        }

        public void Draw()
        {
            DrawingParameters.SpriteBatch.DrawString(Font, Text, DrawingParameters.Position, DrawingParameters.Color, 0, new Vector2(0, 0), Scale, SpriteEffects.None, 0);
        }

        public TextObject(string text, SpriteFont font, Layer layer, Vector2 position, float scale)
        {
            Text = text;
            Font = font;
            Layer = layer;
            Position = position;
            layer.TextObjects.Add(this);
            Scale = scale;
        }

        public TextObject(string text, string fontName, int lineSpacing, float charSpacing, float scale, Layer layer, Vector2 position, params (char letter, Rectangle border, Rectangle cropping)[] characters) :
            this(
                text,
                new SpriteFont(
                    Global.Variables.MainContent.Load<Texture2D>(fontName),
                    characters.Select(c => c.border).ToList(),
                    characters.Select(c => c.cropping).ToList(),
                    characters.Select(c => c.letter).ToList(),
                    lineSpacing,
                    charSpacing,
                    characters.Select(c => new Vector3(0, c.cropping.Width, 0)).ToList(),
                    characters.First().letter
                    ),
                layer,
                position,
                scale
                )
        {
        }

        public TextObject(string text, string fontName, int lineSpacing, float charSpacing, float scale, Layer layer, Vector2 position) :
            this(text, fontName, lineSpacing, charSpacing, scale, layer, position, FontBuilder.BuildFromFiles(fontName))
        {
        }

    }
}
