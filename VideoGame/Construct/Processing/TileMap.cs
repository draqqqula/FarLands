using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace VideoGame
{
    public class TileMapBuilder
    {
        private readonly Dictionary<Color, byte> ColorByteExchanger;
        public byte[,] BuildFromFiles(string level)
        {
            var mapImage = Global.Variables.MainContent.Load<Texture2D>(level);
            Color[] colorMap = new Color[mapImage.Width*mapImage.Height];
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

    public class TileMap
    {
        public readonly Vector2 Position;
        private readonly byte[,] Map;
        private readonly Texture2D Sheet;
        private readonly (Rectangle, Point, bool)[] Tiles;
        public readonly Rectangle TileFrame;
        public readonly Layer Layer;
        public readonly Vector2 Scale;
        public readonly Rectangle[][] VerticalSurfaceMap;


        public Rectangle Frame
        {
            get
            {
                return 
                    new Rectangle(
                        new Point(0, 0), 
                        new Point(Map.GetLength(0)*TileFrame.Width * (int)Scale.X, Map.GetLength(1)*TileFrame.Height * (int)Scale.Y
                    ));
            }
        }

        public TileMap(Vector2 position, string level, string fileName, Rectangle tileFrame, Layer layer, Vector2 scale, int tileCount, params Color[] colors) :
            this(
                position, 
                new TileMapBuilder().BuildFromFiles(level), 
                Global.Variables.MainContent.Load<Texture2D>(fileName), 
                tileFrame,
                layer, 
                scale,
                tileCount,
                colors
                )
        {
        }

        public TileMap(Vector2 position, byte[,] map, Texture2D sheet, Rectangle tileFrame, Layer layer, Vector2 scale, int tileCount, params Color[] colors)
            : 
        this(
            position,
            map,
            sheet,
            tileFrame,
            layer,
            scale,
            Enumerable.Range(0, tileCount).Select(n => (new Rectangle(tileFrame.Location + new Point(tileFrame.Width * n, 0), tileFrame.Size), Point.Zero, true)).ToArray()
        )
        {
        }

        public TileMap(Vector2 position, string level, string fileName, Rectangle tileFrame, Layer layer, Vector2 scale, (Rectangle, Point, bool, Color)[] tiles) :
    this(
        position,
        new TileMapBuilder(tiles.Select(t => t.Item4).ToArray()).BuildFromFiles(level),
        Global.Variables.MainContent.Load<Texture2D>(fileName),
        tileFrame,
        layer,
        scale,
        tiles.Select(t => (t.Item1, t.Item2, t.Item3)).ToArray()
        )
        {
        }

        public TileMap(Vector2 position, byte[,] map, Texture2D sheet, Rectangle tileFrame, Layer layer, Vector2 scale, (Rectangle, Point, bool)[] tiles)
        {
            Position = position;
            Map = map;
            Sheet = sheet;
            TileFrame = tileFrame;
            Layer = layer;
            Scale = scale;
            Tiles = tiles;
            layer.TileMaps.Add(this);
            VerticalSurfaceMap = TileMapBuilder.MakeSurfaceMap(map, tileFrame, scale, tiles.Select(t => t.Item3).ToArray());
        }

        public void Draw()
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (var j = 0; j < Map.GetLength(1); j++)
                {
                    if (Map[i, j] != 0)
                    {
                        var camera = Global.Variables.MainGame._world.CurrentLevel.GameState.Camera;
                        var tileSize = Tiles[Map[i, j] - 1].Item1;
                        var tileOffset = Tiles[Map[i, j] - 1].Item2;
                        var tilePos = Layer.DrawingFunction(Position + new Vector2((i * TileFrame.Width - tileOffset.X) * Scale.Y, (j * TileFrame.Height - tileOffset.Y) * Scale.Y));
                        var tileLayout = new Rectangle(tilePos.ToPoint() + camera.LeftTopCorner.ToPoint(), 
                            new Point(tileSize.Width * (int)Scale.Y, tileSize.Height * (int)Scale.X));
                        if (camera.Window.Intersects(tileLayout))
                        {
                            Global.Variables.MainSpriteBatch.Draw(
                                Sheet,
                                tilePos,
                                tileSize,
                                Color.White,
                                0,
                                new Vector2(0, 0),
                                Scale,
                                SpriteEffects.None,
                                0);
                        }
                    }
                }
            }
        }
    }
}
