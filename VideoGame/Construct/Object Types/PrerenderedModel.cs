using ABI.Windows.Foundation;
using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation.Metadata;
using Point = Microsoft.Xna.Framework.Point;

namespace VideoGame
{
    public class PrerenderedModel : GameObject
    {
        #region READONLY FIELDS
        private readonly Texture2D[] Sheets;
        private readonly float Step;
        private readonly Point Frame;
        private readonly Vector2 Anchor;
        #endregion


        #region DYNAMIC FIELDS
        private Vector3 rotation;
        public Vector3 Rotation
        {
            get { return rotation; }

            set
            {
                rotation = new Vector3(value.X.NormalizeAngle(MathF.PI*2), value.Y.NormalizeAngle(MathF.PI * 2), value.Z.NormalizeAngle(MathF.PI * 2));
            }
        }
        public Vector3 Direction
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
        public bool IsMirrored = false;
        #endregion

        protected override DrawingParameters DisplayInfo
        {
            get
            {
                var rawParameters = new DrawingParameters
                {
                    Position = this.Position,
                    Mirroring = IsMirrored ?
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally : Microsoft.Xna.Framework.Graphics.SpriteEffects.None
                };
                return rawParameters;
            }
        }

        public override IEnumerable<IDisplayable> GetDisplay(GameCamera camera, Layer layer)
        {
            var Zindex = Convert.ToInt32(MathF.Round(Rotation.Z / Step).NormalizeAngle((MathF.PI * 2) / Step));
            yield return new FrameForm(ImageBorder, Anchor, layer.DrawingFunction(DisplayInfo, camera), Sheets[Zindex]);
        }

        private Rectangle ImageBorder
        {
            get
            {
                int offsetX = Convert.ToInt32(MathF.Round(Rotation.X / Step).NormalizeAngle((MathF.PI * 2) / Step));
                int offsetY = Convert.ToInt32(MathF.Round(Rotation.Y / Step).NormalizeAngle((MathF.PI * 2) / Step) * (MathF.PI * 2 / Step));
                return new Rectangle(new Point(Frame.X*(offsetX + offsetY), 0), Frame);
            }
        }

        public PrerenderedModel(Vector2 position, Point frame, float angleStep, Texture2D[] sheets)
        {
            Position = position;
            Frame = frame;
            Box = new Rectangle(new Point(-frame.X/2, -frame.Y/2), frame);
            Anchor = new Point(frame.X / 2, frame.Y / 2).ToVector2();
            Step = angleStep;
            Sheets = sheets;
        }

        public PrerenderedModel(Vector2 position, Point frame, float angleStep, string name, ContentStorage storage) :
            this(position, frame, angleStep, BuildArray(Convert.ToInt32((MathF.PI*2)/angleStep), storage, name))
        {
        }

        private static Texture2D[] BuildArray(int count, ContentStorage storage, string name)
        {
            var result = new List<Texture2D>();
            for (int i = 0; i < count; i++)
            {
                result.Add(storage.GetAsset<Texture2D>(string.Concat(name, i)));
            }
            return result.ToArray();
        }
    }
}
