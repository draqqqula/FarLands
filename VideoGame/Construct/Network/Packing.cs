using System;
using System.Text;
using System.Runtime.Versioning;
using VideoGame;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Buffers.Binary;

namespace Animations
{
    public partial record struct DrawingParameters : IPackable
    {
        public byte[] Pack()
        {
            Span<byte> buffer = stackalloc byte[28];
            MemoryMarshal.Write(buffer, ref Position);
            MemoryMarshal.Write(buffer.Slice(8), ref Color);
            MemoryMarshal.Write(buffer.Slice(12), ref Rotation);
            MemoryMarshal.Write(buffer.Slice(16), ref Scale);
            MemoryMarshal.Write(buffer.Slice(24), ref Priority);
            return buffer.ToArray();
        }

        [RequiresPreviewFeatures]
        public static IPackable Unpack(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 28)
                throw new ArgumentException("Invalid byte array length for DrawingParameters");

            Vector2 position = MemoryMarshal.Read<Vector2>(bytes);
            Color color = MemoryMarshal.Read<Color>(bytes[8..]);
            float rotation = MemoryMarshal.Read<float>(bytes[12..]);
            Vector2 scale = MemoryMarshal.Read<Vector2>(bytes[16..]);
            float priority = MemoryMarshal.Read<float>(bytes[24..]);

            return new DrawingParameters(position, color, rotation, scale, SpriteEffects.None, priority);
        }
    }
    public partial struct FrameForm : IDisplayable, IPackable
    {
        private static SpriteDrawer Drawer;
        public byte[] Pack()
        {
            Span<byte> buffer = stackalloc byte[56];
            BinaryPrimitives.WriteInt32LittleEndian(buffer, Borders.X);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[4..], Borders.Y);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[8..], Borders.Width);
            BinaryPrimitives.WriteInt32LittleEndian(buffer[12..], Borders.Height);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[16..], Anchor.X);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[20..], Anchor.Y);
            BinaryPrimitives.WriteSingleLittleEndian(buffer[24..], 1);
            Arguments.Pack().CopyTo(buffer.Slice(28, 28));
            return buffer.ToArray();
        }

        [RequiresPreviewFeatures]
        public static IPackable Unpack(ReadOnlySpan<byte> bytes)
        {
            if (bytes.Length != 56)
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
            DrawingParameters arguments = (DrawingParameters)DrawingParameters.Unpack(bytes.Slice(28, 28).ToArray());

            return new FrameForm(borders, anchor, arguments, sheetID);
        }
        public FrameForm(Rectangle borders, Vector2 anchor, DrawingParameters arguments, int sheetID) :
    this(borders, anchor, arguments, Drawer.GetSprite(sheetID))
        {
        }
    }
}