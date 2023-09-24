using System;
using System.Text;
using System.Runtime.Versioning;
using VideoGame;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Buffers.Binary;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Windows.Storage.Streams;
using Windows.Media.Protection.PlayReady;
using System.Reflection;
using System.Collections.Immutable;
using Animations;

namespace Animations
{
    [ByteKey(3)]
    public partial record struct DrawingParameters : IPackable
    {

        public IEnumerable<byte> Pack(ContentStorage contentStorage)
        {
            Span<byte> buffer = stackalloc byte[29];
            MemoryMarshal.Write(buffer, ref Position);
            MemoryMarshal.Write(buffer.Slice(8), ref Color);
            MemoryMarshal.Write(buffer.Slice(12), ref Rotation);
            MemoryMarshal.Write(buffer.Slice(16), ref Scale);
            MemoryMarshal.Write(buffer.Slice(24), ref Priority);
            bool mirrored = (Mirroring == SpriteEffects.None);
            MemoryMarshal.Write(buffer.Slice(28), ref mirrored);
            return buffer.ToArray();
        }

        public static DrawingParameters Unpack(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 29)
                throw new ArgumentException("Invalid byte array length for DrawingParameters");

            Vector2 position = MemoryMarshal.Read<Vector2>(bytes);
            Color color = MemoryMarshal.Read<Color>(bytes[8..]);
            float rotation = MemoryMarshal.Read<float>(bytes[12..]);
            Vector2 scale = MemoryMarshal.Read<Vector2>(bytes[16..]);
            float priority = MemoryMarshal.Read<float>(bytes[24..]);
            bool mirrored = MemoryMarshal.Read<bool>(bytes[28..]);
            SpriteEffects mirroring = mirrored ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            return new DrawingParameters(position, color, rotation, scale, mirroring, priority);
        }
    }
    [ByteKey(2)]
    public partial struct FrameForm : IDisplayable, IPackable
    {
        public IEnumerable<byte> Pack(ContentStorage contentStorage)
        {
            Span<byte> buffer = stackalloc byte[57];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Borders.X);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..], Borders.Y);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..], Borders.Width);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[12..], Borders.Height);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[16..], Anchor.X);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[20..], Anchor.Y);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[24..], contentStorage.GetID(Sheet));
            Arguments.Pack(contentStorage).ToArray().CopyTo(buffer.Slice(28, 29));
            return buffer.ToArray();
        }

        public static FrameForm Unpack(ReadOnlySpan<byte> bytes, ContentStorage contentStorage)
        {
            if (bytes.Length != 57)
                throw new ArgumentException("Invalid byte array length for FrameForm");

            Rectangle borders = new Rectangle(
                BinaryPrimitives.ReadInt32LittleEndian(bytes),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[4..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]));
            Vector2 anchor = new Vector2(
                BinaryPrimitives.ReadSingleLittleEndian(bytes[16..]),
                BinaryPrimitives.ReadSingleLittleEndian(bytes[20..]));
            int sheetID = BinaryPrimitives.ReadInt32LittleEndian(bytes[24..]);
            DrawingParameters arguments = DrawingParameters.Unpack(bytes.Slice(28, 29).ToArray());

            return new FrameForm(borders, anchor, arguments, contentStorage.GetAsset<Texture2D>(sheetID));

        }
    }
}

namespace VideoGame
{

    [ByteKey(1)]
    public partial struct TileMapChunk : IPackable, IDisplayable
    {
        public IEnumerable<byte> Pack(ContentStorage contentStorage)
        {
            List<byte> buffer = new();
            var LinkID = Source.LinkedID;
            buffer.AddRange(LinkID);
            buffer.AddRange(BitConverter.GetBytes(Position.X));
            buffer.AddRange(BitConverter.GetBytes(Position.Y));
            buffer.AddRange(BitConverter.GetBytes(Chunk.X));
            buffer.AddRange(BitConverter.GetBytes(Chunk.Y));
            buffer.AddRange(BitConverter.GetBytes(Chunk.Width));
            buffer.AddRange(BitConverter.GetBytes(Chunk.Height));
            buffer.AddRange(BitConverter.GetBytes(Extra.Count()));
            foreach (Point point in Extra)
            {
                buffer.AddRange(BitConverter.GetBytes(point.X));
                buffer.AddRange(BitConverter.GetBytes(point.Y));
            }
            return buffer;
        }
        
        public static TileMapChunk Unpack(ReadOnlySpan<byte> bytes, Dictionary<byte[], GameObject> networkCollection)
        {
            byte[] linkID = bytes[0..16].ToArray();

            Vector2 position = new Vector2(
                BinaryPrimitives.ReadSingleLittleEndian(bytes[16..]),
                BinaryPrimitives.ReadSingleLittleEndian(bytes[20..])
                );
            Rectangle chunk = new Rectangle(
                BinaryPrimitives.ReadInt32LittleEndian(bytes[24..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[28..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[32..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[36..])
                );
            Point[] extra = new Point[BinaryPrimitives.ReadInt32LittleEndian(bytes[40..])];
            for(int i = 0; i < extra.Length; i++)
            {
                extra[i] = new Point(
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(44 + i * 8)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(48 + i * 8)..])
                    );
            }
            return new TileMapChunk(networkCollection[linkID] as TileMap, chunk, position, extra);
        }
    }

    [ByteKey(0)]
    public partial class TileMap : IPackable
    {
        public IEnumerable<byte> Pack(ContentStorage contentStorage)
        {
            List<byte> buffer = new List<byte>();
            
            buffer.AddRange(BitConverter.GetBytes(Position.X));
            buffer.AddRange(BitConverter.GetBytes(Position.Y));
            buffer.AddRange(BitConverter.GetBytes(SheetID));
            buffer.AddRange(BitConverter.GetBytes(TileFrame.X));
            buffer.AddRange(BitConverter.GetBytes(TileFrame.Y));
            buffer.AddRange(BitConverter.GetBytes(TileFrame.Width));
            buffer.AddRange(BitConverter.GetBytes(TileFrame.Height));
            buffer.AddRange(BitConverter.GetBytes(PictureScale.X));
            buffer.AddRange(BitConverter.GetBytes(PictureScale.Y));
            var linkID = LinkedID;
            buffer.AddRange(linkID);
            buffer.AddRange(BitConverter.GetBytes(Tiles.Length));
            foreach (var tile in Tiles)
            {
                buffer.AddRange(BitConverter.GetBytes(tile.Item1.X));
                buffer.AddRange(BitConverter.GetBytes(tile.Item1.Y));
                buffer.AddRange(BitConverter.GetBytes(tile.Item1.Width));
                buffer.AddRange(BitConverter.GetBytes(tile.Item1.Height));
                buffer.AddRange(BitConverter.GetBytes(tile.Item2.X));
                buffer.AddRange(BitConverter.GetBytes(tile.Item2.Y));
                buffer.AddRange(BitConverter.GetBytes(tile.Item3));
            }

            buffer.AddRange(BitConverter.GetBytes(Map.GetLength(0)));
            buffer.AddRange(BitConverter.GetBytes(Map.GetLength(1)));

            for (int i = 0; i < Map.GetLength(0); i++)
            {
                for (int j = 0; j < Map.GetLength(1); j++)
                {
                    buffer.Add(Map[i,j]);
                }
            }

            return buffer.ToArray();
        }

        private static byte[,] BuildMap(ReadOnlySpan<byte> bytes, int rows, int columns)
        {
            byte[,] map = new byte[rows, columns];
            int index = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    map[i, j] = bytes[index];
                    index++;
                }
            }

            return map;
        }

        public static TileMap Unpack(ReadOnlySpan<byte> bytes, ContentStorage contentStorage, Layer layer)
        {
            Vector2 position = new Vector2(BinaryPrimitives.ReadSingleLittleEndian(bytes), BinaryPrimitives.ReadSingleLittleEndian(bytes[4..]));
            Texture2D sheet = contentStorage.GetAsset<Texture2D>(BinaryPrimitives.ReadInt32LittleEndian(bytes[8..]));
            Rectangle tileFrame = new Rectangle(
                BinaryPrimitives.ReadInt32LittleEndian(bytes[12..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[16..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[20..]),
                BinaryPrimitives.ReadInt32LittleEndian(bytes[24..])
                );
            Vector2 pictureScale = new Vector2(BinaryPrimitives.ReadSingleLittleEndian(bytes[28..]), BinaryPrimitives.ReadSingleLittleEndian(bytes[32..]));
            byte[] linkID = bytes.Slice(36,16).ToArray();


            (Rectangle, Point, bool)[] tiles = new (Rectangle, Point, bool)[BinaryPrimitives.ReadInt32LittleEndian(bytes[52..])];
            int tileLoopStart = 56;
            for (int n= 0; n < tiles.Length; n++)
            {
                Rectangle frame = new Rectangle(
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25 + 4)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25 + 8)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25 + 12)..])
                    );
                Point offset = new Point(
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25 + 16)..]),
                    BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopStart + n * 25 + 20)..])
                    );
                bool isSolid = MemoryMarshal.Read<bool>(bytes[(tileLoopStart + n * 25 + 24)..]);
                tiles[n] = new(frame, offset, isSolid);
            }
            int tileLoopEnd = tileLoopStart + tiles.Length * 25;

            int mapLoopStart = tileLoopEnd + 8;

            int mapWidth = BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopEnd)..]);
            int mapHeight = BinaryPrimitives.ReadInt32LittleEndian(bytes[(tileLoopEnd + 4)..]);

            ReadOnlySpan<byte> rawMap = bytes[mapLoopStart..];
            byte[,] map = BuildMap(bytes[mapLoopStart..], mapWidth, mapHeight);

            var obj = new TileMap(position, map, sheet, contentStorage, tileFrame, layer, pictureScale, tiles);
            obj.Link(linkID);
            return obj;
        }
    }

    public abstract partial class GameState
    {
        public IEnumerable<byte> GetInitialPack(ContentStorage contentStorage)
        {
            List<byte> buffer = new();
            var tileMaps = Layers.SelectMany(it => it.Value).Where(it => it is TileMap).Select(it => it as TileMap);
            int c = 0;
            foreach(var map in tileMaps)
            {
                map.Link(BitConverter.GetBytes(c).Concat(BitConverter.GetBytes(c)).Concat(BitConverter.GetBytes(c)).Concat(BitConverter.GetBytes(c)).ToArray());
                IEnumerable<byte> mapBytes = map.Pack(contentStorage);
                buffer.AddRange(BitConverter.GetBytes(mapBytes.Count()));
                buffer.AddRange(mapBytes);
                c++;
            }
            return buffer;
        }

        public IEnumerable<TileMap> UnpackTileMaps(byte[] bytes, Layer layer)
        {
            int pointer = 0;
            while (pointer < bytes.Length)
            {
                int length = BinaryPrimitives.ReadInt32LittleEndian(bytes[pointer..]);
                var obj = TileMap.Unpack(bytes[(pointer+4)..(pointer + 4 + length)], ContentStorage, layer);
                pointer += length + 4;
                yield return obj;
            }
        }

        public IEnumerable<byte> GetPack(GameClient client)
        {
            List<byte> buffer = new ();
            var camera = GetCamera(client);
            foreach (var layer in Layers.Values)
            {
                var view = layer.PointsOfView[camera];

                foreach (var drawable in view.Pictures)
                {
                    if (drawable is IPackable packable)
                    {
                        var pack = packable.Pack(ContentStorage);
                        buffer.Add(packable.GetType().GetCustomAttribute<ByteKeyAttribute>().value);
                        buffer.AddRange(BitConverter.GetBytes(pack.Count()));
                        buffer.AddRange(pack);
                    }
                }
            }

            return buffer.ToArray();
        }


        private static ImmutableArray<Type> PackableTypes = Assembly.GetAssembly(typeof(GameState))
            .GetTypes()
            .Where(type => type.GetInterfaces().Contains(typeof(IPackable))).OrderBy(it => it.GetCustomAttribute<ByteKeyAttribute>().value).ToImmutableArray();

        public static IEnumerable<IDisplayable> Unpack(byte[] bytes, ContentStorage contentStorage, Dictionary<byte[], GameObject> networkCollection)
        {
            int pointer = 0;
            while (pointer < bytes.Length)
            {
                Type type = PackableTypes[bytes[pointer]];
                int length = BinaryPrimitives.ReadInt32LittleEndian(bytes[(pointer+1)..]);
                IDisplayable obj = null;

                if (type == typeof(FrameForm))
                {
                    obj = FrameForm.Unpack(bytes.AsSpan<byte>()[(pointer + 5)..(pointer + 5 + length)], contentStorage);
                }
                else if (type == typeof(TileMapChunk))
                {

                        obj = TileMapChunk.Unpack(bytes.AsSpan<byte>()[(pointer + 5)..(pointer + 5 + length)], networkCollection);

                }

                pointer += length+5;

                if (obj is null)
                {
                    throw new ArgumentException("Cannot decide type of encoded object");
                }
                yield return obj;
            }
            yield break;
        }
    }
}