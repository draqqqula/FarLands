using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class SpriteDrawer
    {
        private readonly ContentManager Content;
        private readonly GraphicsDevice Device;
        private readonly Dictionary<string, Texture2D> Data = new Dictionary<string, Texture2D>();
        private readonly Dictionary<int, string> IDExchanger = new Dictionary<int, string>();

        public Texture2D GetSprite(string name)
        {
            if (Data.ContainsKey(name))
            {
                return Data[name];
            }
            else
            {
                Texture2D image;
                try
                {
                    image = Content.Load<Texture2D>(name);
                }
                catch
                {
                    image = GenerateMissingTexture(Device, Color.Black, Color.MediumPurple, 2);
                }
                Data.Add(name, image);
                return image;
            }
        }

        public Texture2D GetSprite(int id)
        {
            return GetSprite(IDExchanger[id]);
        }

        public void DrawFromString()
        {
            throw new NotImplementedException();
        }

        public void PreLoadAssets(params string[] names)
        {
            foreach (var name in names)
            {
                Data[name] = Content.Load<Texture2D>(name);
            }
        }

        public SpriteDrawer(GraphicsDevice device, ContentManager content)
        {
            Content = content;
            Device = device;
        }

        private Texture2D GenerateMissingTexture(GraphicsDevice device, Color goodColor, Color badColor, int size)
        {
            Color[] colors = new Color[size*size];
            for (int i = 0; i < size*size; i++)
            {
                if (i%2 == 0) colors[i] = goodColor;
                else colors[i] = badColor;
            }
            Texture2D texture = new Texture2D(device, 2, 2);
            texture.SetData(colors);
            return texture;
        }

        public void Draw(IEnumerable<Layer> layers, GameCamera camera, SpriteBatch spriteBatch)
        {
            foreach (var layer in layers)
            {
                var view = layer.PointsOfView[camera];

                foreach (var drawable in view.Pictures)
                {
                    drawable.Draw(spriteBatch, camera, this);
                }
            }
        }
    }
}
