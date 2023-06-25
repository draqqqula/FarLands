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
    /// <summary>
    /// воссоздаёт карту тайлов по файлам
    /// </summary>
    public class TileMapBuilder
    {
        private readonly Dictionary<Color, byte> ColorByteExchanger;
        /// <summary>
        /// загружает картинку и превращает её в массив байтов
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public byte[,] BuildFromFiles(string level, ContentManager content)
        {
            var mapImage = content.Load<Texture2D>(level);
            Color[] colorMap = new Color[mapImage.Width * mapImage.Height];
            mapImage.GetData(colorMap);
            var rawResult = colorMap.Select(color => ColorByteExchanger[color]).ToArray();
            var result = new byte[mapImage.Width, mapImage.Height];
            for (int i = 0; i < mapImage.Width; i++)
            {
                for (int j = 0; j < mapImage.Height; j++)
                {
                    result[i, j] = rawResult[j * mapImage.Width + i];
                }
            }
            return result;
        }

        /// <summary>
        /// Преобразует твёрдые тайлы в набор вертикальных препятствий, отсортированных в порядке возрастания координаты X
        /// </summary>
        /// <param name="map"></param>
        /// <param name="frame"></param>
        /// <param name="scale"></param>
        /// <param name="tileStates"></param>
        /// <returns></returns>
        public static Rectangle[][] MakeSurfaceMap(byte[,] map, Rectangle frame, Vector2 scale, bool[] tileStates)
        {
            var result = new List<Rectangle>[map.GetLength(0)].Select(e => new List<Rectangle>()).ToArray();
            for (int i = 0; i < map.GetLength(0); i++)
            {
                int tileCounter = 0;

                for (int j = 0; j <= map.GetLength(1); j++)
                {
                    if (j < map.GetLength(1) && (map[i, j] != 0 && tileStates[map[i, j] - 1]))
                        tileCounter += 1;
                    else if (tileCounter > 0)
                    {
                        Point size = new Point(frame.Width * (int)scale.X, tileCounter * frame.Height * (int)scale.Y);
                        Point position = new Point(frame.Width * (int)scale.X * i, frame.Height * (int)scale.Y * (j - tileCounter));
                        result[i].Add(new Rectangle(position, size));
                        tileCounter = 0;
                    }
                }
            }
            return result.Select(e => e.ToArray()).ToArray();
        }

        public TileMapBuilder(params Color[] colors)
        {
            if (colors.Length == 0)
            {
                colors = new Color[8] {
                Color.Black,
                Color.Red,
                Color.Orange,
                Color.Yellow,
                Color.Green,
                Color.LightBlue,
                Color.Blue,
                Color.Purple
            };
            }
            ColorByteExchanger = Enumerable.Range(0, colors.Length).ToDictionary(n => colors[n], n => (byte)(n + 1));
            ColorByteExchanger.Add(Color.White, 0);
        }

        public TileMapBuilder() :
            this(new Color[0])
        {
        }
    }
}
