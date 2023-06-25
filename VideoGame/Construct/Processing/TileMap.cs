using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace VideoGame
{
    /// <summary>
    /// неизменяемая сетка тайлов
    /// </summary>
    public class TileMap
    {
        public readonly Vector2 Position;
        private readonly byte[,] Map;
        private readonly Texture2D Sheet;
        private readonly (Rectangle, Point, bool)[] Tiles;
        /// <summary>
        /// шаг сетки тайлов
        /// </summary>
        public readonly Rectangle TileFrame;
        public readonly Layer Layer;
        /// <summary>
        /// масштаб, применяемый и к отрисовке и к физическому представлению
        /// </summary>
        public readonly Vector2 Scale;
        /// <summary>
        /// все твёрдые поверхности на карте, отсортированные в порядке возрастания координаты X
        /// </summary>
        public readonly Rectangle[][] VerticalSurfaceMap;

        /// <summary>
        /// расположение площади, занимаемой картой
        /// </summary>
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

        public TileMap(ContentManager content, Vector2 position, string level, string fileName, Rectangle tileFrame, Layer layer, Vector2 scale, int tileCount, params Color[] colors) :
            this(
                position, 
                new TileMapBuilder().BuildFromFiles(level, content), 
                content.Load<Texture2D>(fileName), 
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

        public TileMap(ContentManager content, Vector2 position, string level, string fileName, Rectangle tileFrame, Layer layer, Vector2 scale, (Rectangle, Point, bool, Color)[] tiles) :
    this(
        position,
        new TileMapBuilder(tiles.Select(t => t.Item4).ToArray()).BuildFromFiles(level, content),
        content.Load<Texture2D>(fileName),
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

        /// <summary>
        /// отображает карту тайлов на слое
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, GameCamera camera)
        {
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (var j = 0; j < Map.GetLength(1); j++)
                {
                    if (Map[i, j] != 0)
                    {
                        var tileSize = Tiles[Map[i, j] - 1].Item1;
                        var tileOffset = Tiles[Map[i, j] - 1].Item2;
                        var tilePos = Layer.DrawingFunction(Position + new Vector2((i * TileFrame.Width - tileOffset.X) * Scale.Y, (j * TileFrame.Height - tileOffset.Y) * Scale.Y));
                        var tileLayout = new Rectangle(tilePos.ToPoint() + camera.LeftTopCorner.ToPoint(), 
                            new Point(tileSize.Width * (int)Scale.Y, tileSize.Height * (int)Scale.X));
                        if (camera.Window.Intersects(tileLayout))
                        {
                            spriteBatch.Draw(
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
