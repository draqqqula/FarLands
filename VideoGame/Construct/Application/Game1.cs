using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using Animations;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Net.Sockets;
using VideoGame.Construct;

namespace VideoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D HitBoxTexture;
        public World _world;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GraphicsDevice.SetRenderTarget(new RenderTarget2D(GraphicsDevice, 1920, 1080));
            _graphics.PreferredBackBufferWidth = 1903;
            _graphics.PreferredBackBufferHeight = 969;
            _graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 120f);
            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;
            Window.Title = "Farland";


            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            Global.Variables.MainGame = this;
            Global.Variables.MainSpriteBatch = _spriteBatch;

            HitBoxTexture = Content.Load<Texture2D>("HitBox");

            _spriteBatch.Begin();

            _world = new World();
            _world.AddLevel(new Level("Level1", LevelConstructors.LoadLevel1));
            _world.AddLevel(new Level("Level2", LevelConstructors.LoadLevel2));
            _world.AddLevel(new Level("Level3", LevelConstructors.LoadLevel3));
            _world.LoadLevel("Level1");


            _spriteBatch.End();
        }

        protected override void Update(GameTime gameTime)
        {
            Global.Variables.DeltaTime = gameTime.ElapsedGameTime;

            _world.Update();

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (_world.CurrentLevel != null)
            {
                foreach (var layer in _world.CurrentLevel.GameState.Layers.Values)
                {
                    foreach (var drawable in layer.DrawBuffer.Values)
                        drawable.Draw();
                    foreach (var tileMap in layer.TileMaps)
                        tileMap.Draw();
                    foreach (var text in layer.TextObjects)
                        text.Draw();
                    layer.DrawBuffer.Clear();
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}