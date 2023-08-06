using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace VideoGame
{
    /// <summary>
    /// текстовый объект
    /// </summary>
    public class TextObject : GameObject, IDisplayable
    {
        public string Text { get; set; }

        public SpriteFont Font { get; set; }

        public float TextScale { get; set; }


        protected override DrawingParameters DisplayInfo
        {
            get
            {
                return new DrawingParameters
                {
                    Position = this.Position,
                    Color = Color.White,
                };
            }
        }

        #region IDISPLAYABLE
        public bool IsImmutable => false;

        public void Draw(SpriteBatch spriteBatch, GameCamera camera, SpriteDrawer streamDrawer)
        {
            var arguments = PresentLayer.DrawingFunction(DisplayInfo, camera);
            spriteBatch.DrawString(Font, Text,
                arguments.Position,
                DisplayInfo.Color, 0, new Vector2(0, 0), TextScale, arguments.Mirroring, 0);
        }

        public override IDisplayable GetVisualPart(GameCamera camera)
        {
            return this;
        }

        #endregion

        public TextObject(string text, SpriteFont font, Layer layer, Vector2 position, float scale)
        {
            Text = text;
            Font = font;
            PresentLayer = layer;
            Position = position;
            layer.Add(this);
            TextScale = scale;
        }

        public TextObject(ContentManager content, string text, string fontName, int lineSpacing, float charSpacing, float scale, Layer layer, Vector2 position, params (char letter, Rectangle border, Rectangle cropping)[] characters) :
            this(
                text,
                new SpriteFont(
                    content.Load<Texture2D>(fontName),
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

        public TextObject(ContentManager content, string text, string fontName, int lineSpacing, float charSpacing, float scale, Layer layer, Vector2 position) :
            this(content, text, fontName, lineSpacing, charSpacing, scale, layer, position, FontBuilder.BuildFromFiles(fontName))
        {
        }

    }
}
