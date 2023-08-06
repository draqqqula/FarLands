using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using VideoGame.Construct;
using Windows.UI.StartScreen;

namespace VideoGame
{
    /// <summary>
    /// неизменяемая сетка тайлов
    /// </summary>
    public class TileMap : GameObject, IDisplayable
    {
        #region DATA

        private readonly byte[,] Map;
        private readonly Texture2D Sheet;
        private readonly (Rectangle, Point, bool)[] Tiles;
        private readonly IEnumerable<Point> ExtraPoints;
        /// <summary>
        /// шаг сетки тайлов
        /// </summary>
        public readonly Rectangle TileFrame;
        /// <summary>
        /// масштаб, применяемый и к отрисовке и к физическому представлению
        /// </summary>
        public readonly Vector2 PictureScale;
        /// <summary>
        /// все твёрдые поверхности на карте, отсортированные в порядке возрастания координаты X
        /// </summary>
        public readonly Rectangle[][] VerticalSurfaceMap;

        /// <summary>
        /// расположение площади, занимаемой картой
        /// </summary>
        public override Rectangle Box
        {
            get
            {
                return 
                    new Rectangle(
                        new Point(0, 0), 
                        new Point(Map.GetLength(0)*TileFrame.Width * (int)PictureScale.X, Map.GetLength(1)*TileFrame.Height * (int)PictureScale.Y
                    ));
            }
        }
        #endregion

        #region CONSTRUCTORS

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
            PresentLayer = layer;
            PictureScale = scale;
            Tiles = tiles;

            HashSet<int> OddShaped = new ();
            for (int n = 0; n < tiles.Length; n++)
            {
                if (tiles[n].Item1.Size != tileFrame.Size)
                {
                    OddShaped.Add(n+1);
                }
            }
            ExtraPoints = GetPoints(OddShaped);



            layer.Add(this);

            VerticalSurfaceMap = TileMapBuilder.MakeSurfaceMap(map, tileFrame, scale, tiles.Select(t => t.Item3).ToArray());
        }

        private IEnumerable<Point> GetPoints(HashSet<int> tiles)
        {
            List<Point> points = new();
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = 0; j < Map.GetLength(1); j++)
                {
                    if (tiles.Contains(Map[i, j]))
                    {
                        points.Add(new Point(i, j));
                    }
                }
            }
            return points;
        }

        #endregion

        #region IDISPLAYABLE
        public bool IsImmutable => true;
        public void Draw(SpriteBatch spriteBatch, GameCamera camera, SpriteDrawer streamDrawer)
        {
            var pos = PresentLayer.DrawingFunction(DisplayInfo, camera).Position;
            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (var j = 0; j < Map.GetLength(1); j++)
                {
                    if (Map[i, j] != 0)
                    {
                        var tileSize = Tiles[Map[i, j] - 1].Item1;
                        var tileOffset = Tiles[Map[i, j] - 1].Item2;
                        var tilePos = pos + new Vector2((i * TileFrame.Width - tileOffset.X) * PictureScale.Y, (j * TileFrame.Height - tileOffset.Y) * PictureScale.Y);
                        var tileLayout = new Rectangle(tilePos.ToPoint() + camera.LeftTopCorner.ToPoint(), 
                            new Point(tileSize.Width * (int)PictureScale.Y, tileSize.Height * (int)PictureScale.X));
                        if (camera.ViewPort.Intersects(tileLayout))
                        {
                            spriteBatch.Draw(
                                Sheet,
                                tilePos,
                                tileSize,
                                Color.White,
                                0,
                                new Vector2(0, 0),
                                PictureScale,
                                SpriteEffects.None,
                                0);
                        }
                    }
                }
            }
        }

        public void DrawChunk(SpriteBatch spriteBatch, SpriteDrawer drawer, Rectangle chunk, IEnumerable<Point> extra, Vector2 position)
        {
            for (int i = chunk.Left; i <= chunk.Right; i++)
            {
                for (int j = chunk.Top; j <= chunk.Bottom; j++)
                {
                    if (Map[i, j] != 0)
                    {
                        DrawTile(i, j, position, spriteBatch);
                    }
                }
            }
            foreach (var point in extra)
            {
                DrawTile(point.X, point.Y, position, spriteBatch);
            }
        }

        private void DrawTile(int x, int y, Vector2 position, SpriteBatch batch)
        {

            var tileSize = Tiles[Map[x, y] - 1].Item1;
            var tileOffset = Tiles[Map[x, y] - 1].Item2;
            var tilePos = position + new Vector2((x * TileFrame.Width - tileOffset.X) * PictureScale.Y, (y * TileFrame.Height - tileOffset.Y) * PictureScale.Y);
            batch.Draw(
                        Sheet,
                        tilePos,
                        tileSize,
                        Color.White,
                        0,
                        new Vector2(0, 0),
                        PictureScale,
                        SpriteEffects.None,
                        0);
        }

        public override IDisplayable GetVisualPart(GameCamera camera)
        {
            var pos = PresentLayer.DrawingFunction(DisplayInfo, camera).Position;
            var chunk = GetChunk(new Rectangle((-pos).ToPoint(), camera.ViewPort.Size));
            return new TileMapChunk(this, chunk, pos,
                ExtraPoints.Where(
                    (it) => 
                    {
                        if (chunk.Contains(it)) return false;
                        var tileSize = Tiles[Map[it.X, it.Y] - 1].Item1;
                        var tileOffset = Tiles[Map[it.X, it.Y] - 1].Item2;
                        var tilePos = pos + new Vector2((it.X * TileFrame.Width - tileOffset.X) * PictureScale.Y, (it.Y * TileFrame.Height - tileOffset.Y) * PictureScale.Y);
                        var tileLayout = new Rectangle(tilePos.ToPoint() + camera.LeftTopCorner.ToPoint(),
                            new Point(tileSize.Width * (int)PictureScale.Y, tileSize.Height * (int)PictureScale.X));
                        return (camera.ViewPort.Intersects(tileLayout));
                    }
                )
                );
        }

        private Rectangle GetChunk(Rectangle window)
        {
            int width = Map.GetLength(0)-1;
            int height = Map.GetLength(1)-1;
            var frame = TileFrame.Size * PictureScale.ToPoint();
            int startX = Math.Clamp(window.Left / frame.X, 0, width);
            int startY = Math.Clamp(window.Top / frame.Y, 0, height);
            int endX = Math.Clamp(window.Right / frame.X, 0, width);
            int endY = Math.Clamp(window.Bottom / frame.Y, 0, height);
            return new Rectangle(startX, startY, endX - startX, endY - startY);
        }
        #endregion
    }

    public struct TileMapChunk : IDisplayable
    {
        private TileMap Source;
        private Vector2 Position;
        private Rectangle Chunk;
        private IEnumerable<Point> Extra;

        public bool IsImmutable => false;

        public void Draw(SpriteBatch batch, GameCamera camera, SpriteDrawer drawer)
        {
            Source.DrawChunk(batch, drawer, Chunk, Extra, Position);
        }

        public TileMapChunk(TileMap source, Rectangle chunk, Vector2 position, IEnumerable<Point> extra)
        {
            Source = source;
            Chunk = chunk;
            Position = position;
            Extra = extra;
        }
    }
}
