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
using Windows.Globalization;
using System.Reflection;
using System.Linq;
using Windows.Devices.Sms;
using System.Windows.Forms;
using WinRT;
using Keys = Microsoft.Xna.Framework.Input.Keys;
using NUnit.Framework.Internal;

namespace VideoGame
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch; 
        public World _world;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferHalfPixelOffset = true;
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            GraphicsDevice.SetRenderTarget(new RenderTarget2D(
                GraphicsDevice, 1920, 1080,
                false, SurfaceFormat.Color,
                DepthFormat.Depth24, 6,
                RenderTargetUsage.DiscardContents));

            _graphics.PreferredBackBufferWidth = 1903;
            _graphics.PreferredBackBufferHeight = 969;
            _graphics.PreferMultiSampling = true;
            _graphics.SynchronizeWithVerticalRetrace = false;
            this.IsFixedTimeStep = false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1 / 60f);
            _graphics.ApplyChanges();
            Window.AllowUserResizing = true;
            Window.Title = "Farland";

            DepthStencilState depthStencilState; depthStencilState = new DepthStencilState();
            depthStencilState.DepthBufferFunction = CompareFunction.LessEqual;
            depthStencilState.DepthBufferWriteEnable = true;
            depthStencilState.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = depthStencilState;

            GraphicsDevice.Reset();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _spriteBatch.Begin();

            _world = 
                new World(
                    new GameClient(Window.ClientBounds, LevelConstructors.CreateKeyBoardControls(), GameClient.GameLanguage.English),
                    new ContentStorage(GraphicsDevice, Content
                ));

            _world.AddLevel(new Level("Level1", LevelConstructors.LoadLevel1));
            _world.AddLevel(new Level("Remote Room", LevelConstructors.LoadRemoteRoom));
            _world.AddLevel(new Level("Menu", LevelConstructors.LoadMenu));
            _world.LoadRootLevel("Level1", Content);
            _spriteBatch.End();
        }
        protected override void Update(GameTime gameTime)
        {
            _world.Update(gameTime.ElapsedGameTime, Window);

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) 
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            if (_world.IsReady)
            {
                _world.Display(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);

        }
    }
}