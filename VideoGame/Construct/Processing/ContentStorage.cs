using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class ContentStorage
    {
        private readonly ContentManager ContentProvider;
        private readonly GraphicsDevice Device;
        private readonly Dictionary<string, object> Data = new Dictionary<string, object>();
        private readonly Dictionary<object, string> ReverseData = new Dictionary<object, string>();
        private readonly string[] IDExchanger;
        private readonly Dictionary<string, int> ReverseIDExchanger;

        public T GetAsset<T>(string name) where T : class
        {
            if (Data.ContainsKey(name))
            {
                return Data[name] as T;
            }
            else
            {
                T asset;
                try
                {
                    asset = ContentProvider.Load<T>(name);
                }
                catch
                {
                    throw new Exception("Asset not found");
                }
                Data.Add(name, asset);
                ReverseData.Add(asset, name);
                return asset;
            }
        }

        public T GetAsset<T>(int id) where T : class
        {
            return GetAsset<T>(IDExchanger[id]);
        }

        public string GetName(object asset)
        {
            return ReverseData[asset];
        }

        public int GetID(object asset)
        {
            return ReverseIDExchanger[GetName(asset)];
        }

        public void DrawFromString()
        {
            throw new NotImplementedException();
        }

        public void PreLoadAssets(params string[] names)
        {
            foreach (var name in names)
            {
                Data[name] = ContentProvider.Load<Texture2D>(name);
            }
        }

        public ContentStorage(GraphicsDevice device, ContentManager content)
        {
            ContentProvider = content;
            Device = device;
            IDExchanger = Directory.CreateDirectory(content.RootDirectory)
                .GetFiles()
                .Select(it => Path.GetFileNameWithoutExtension(it.Name))
                .OrderBy(it => it).ToArray();
            ReverseIDExchanger = Enumerable.Range(0, IDExchanger.Length).ToDictionary(it => IDExchanger[it]);
        }

        private object GenerateMissingTexture(GraphicsDevice device, Color goodColor, Color badColor, int size)
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

        private static Color GenerateRandomColor()
        {
            Random random = new Random();
            Vector4 vector = new (random.NextSingle(), random.NextSingle(), random.NextSingle(), 1.0f);
            return Color.FromNonPremultiplied(vector);
        }
    }
}
