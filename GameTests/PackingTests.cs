using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NUnit.Framework;
using System.Runtime.Versioning;


namespace GameTests
{
    [TestFixture]
    internal class PackingTests
    {
        [RequiresPreviewFeatures]
        [Test]
        public void TestPackAndUnpack()
        {
            Rectangle borders = new (10, 20, 30, 40);
            Vector2 anchor = new (0.5f, 0.5f);
            DrawingParameters arguments = new (
                new Vector2(100, 200),
                Color.Red,
                0.5f,
                new Vector2(1.0f, 1.0f),
                SpriteEffects.None,
                0.0f);

            throw new NotImplementedException();
        }
    }
}
