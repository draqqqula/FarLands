using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Reflection.Emit;
using VideoGame.Construct.Behaviors;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics.Metrics;
using VideoGame.Construct;
using Microsoft.Xna.Framework.Content;
using Animations;
using Windows.Networking.Connectivity;
using System.Runtime.CompilerServices;
using VideoGame.Construct.Families;
using VideoGame.Construct.Patterns;

namespace VideoGame
{
    public static class LevelConstructors
    {
        public static GameControls CreateKeyBoardControls()
        {
            var keyboard_controls = new GameControls();
            keyboard_controls.ChangeControl(Control.left, () => Keyboard.GetState().IsKeyDown(Keys.A));
            keyboard_controls.ChangeControl(Control.right, () => Keyboard.GetState().IsKeyDown(Keys.D));
            keyboard_controls.ChangeControl(Control.jump, () => Keyboard.GetState().IsKeyDown(Keys.Space));
            keyboard_controls.ChangeControl(Control.dash, () => Keyboard.GetState().IsKeyDown(Keys.LeftShift));
            keyboard_controls.ChangeControl(Control.pause, () => Keyboard.GetState().IsKeyDown(Keys.Escape));
            return keyboard_controls;
        }

        public static void CreateSkyAndClouds(Layer skyLayer, Layer cloudsLayer, Layer cloudsBackLayer, ContentManager content)
        {
            new TileMap(Vector2.Zero, new byte[4, 4] {
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 } },
                content.Load<Texture2D>("Sky"),
            new Rectangle(0, 0, 396, 100), skyLayer, new Vector2(3, 3), 3);

            new TileMap(new Vector2(0, 400), new byte[4, 1] {
                { 1 },
                { 1 },
                { 1 },
                { 1 } },
                content.Load<Texture2D>("Clouds"),
                new Rectangle(0, 0, 439, 115), cloudsLayer, new Vector2(3, 3), 3);

            new TileMap(new Vector2(-150, 496), new byte[4, 1] {
                { 1 },
                { 1 },
                { 1 },
                { 1 } },
                content.Load<Texture2D>("Clouds"),
                new Rectangle(0, 0, 439, 115), cloudsBackLayer, new Vector2(3, 3), 3);
        }

        public static TileMap CreateForestTilemap(Vector2 position, string levelName, Layer layer, ContentManager content)
        {
            return new TileMap(content, position, levelName, "rocks", new Rectangle(0, 0, 12, 12), layer, new Vector2(3, 3),
                new (Rectangle, Point, bool, Color)[]
                {
                    (new Rectangle(0, 0, 12, 12), new Point(0, 0), true, Color.Black),
                    (new Rectangle(12, 0, 12, 12), new Point(0, 0), true, new Color(255, 242, 0)),
                    (new Rectangle(24, 0, 12, 12), new Point(0, 0), true, new Color(255, 201, 14)),
                    (new Rectangle(110, 0, 49, 83), new Point(25, 83 - 12), false, new Color(127, 127, 127)),
                    (new Rectangle(163, 12, 51, 71), new Point(25, 71 - 12), false, new Color(195, 195, 195)),
                    (new Rectangle(224, 0, 18, 85), new Point(9, 85 - 12), false, new Color(185, 122, 87)),
                    (new Rectangle(252, 5, 46, 78), new Point(23, 78 - 12), false, new Color(82, 82, 82)),
                    (new Rectangle(162, 12, 51, 71), new Point(25, 71 - 12), false, new Color(226, 226, 226)),
                    (new Rectangle(307, 8, 50, 75), new Point(25, 75 - 12), false, new Color(53, 53, 53))
                });
        }



        public static GameState LoadMenu(World world, ContentManager content, string levelName)
        {
            var state = new MenuState(false);
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(world.Drawer);
            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600), world.Client);
            Layer centerBound = new Layer("CenterBound", (pos, cam) => cam.ApplyParalax(pos, 1, 1), 0.1);
            new TextObject(content, "Paused", "pixel", 0, 3f, 3f, centerBound, new Vector2(0, 0));
            state.Layers.Add("CenterBound", centerBound);
            return state;
        }

        /// <summary>
        /// Инициализирует первый уровень. На нём игрок обучается базовым механикам передвижения
        /// </summary>
        /// <returns></returns>
        public static GameState LoadLevel1(World world, ContentManager content, string levelName)
        {
            Vector2 exitPosition = new Vector2(5800, 1200);
            Vector2 startPosition = new Vector2(300, 300);

            var state = new LocationState(false);
            state.Content = content;
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(world.Drawer);

            Layer mainLayer = new Layer("Main", (pos, cam) => cam.ApplyParalax(pos, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", (pos, cam) => pos, 0);
            Layer surfacesLayer = new Layer("Surfaces", (pos, cam) => cam.ApplyParalax(pos, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", (pos, cam) => cam.ApplyParalax(pos, 0.07f, 0.03f), 0.1);
            Layer cloudsBackLayer = new Layer("CloudsBack", (pos, cam) => cam.ApplyParalax(pos, 0.13f, 0.07f), 0.15);
            Layer particlesFrontLayer = new Layer("ParticlesFront", (pos, cam) => cam.ApplyParalax(pos, 1, 1), 0.3);
            Layer particlesBackLayer = new Layer("ParticlesBack", (pos, cam) => cam.ApplyParalax(pos, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", (pos, cam) => pos, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", (pos, cam) => new Vector2(cam.ViewPort.Width, cam.ViewPort.Height) - pos, 1);
            Layer leftTopBound = new Layer("LeftTopBound", (pos, cam) => pos, 1);
            state.MainLayer = mainLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, cloudsBackLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);


            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level1", surfacesLayer, content);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer, cloudsBackLayer, content);

            var entities = state.GetFamily<Entity>();

            Hood hood = new Hood(state, new Vector2(4000, 1000), mainLayer, true, state.MainTileMap, entities);
            Gate gate = new Gate(state, exitPosition, new Rectangle(-2, -250, 5, 500), mainLayer, "Level2", state.Players);

            new TextObject(content, "Controls", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 500));
            new TextObject(content, "A Left", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 550));
            new TextObject(content, "D Right", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 600));
            new TextObject(content, "Space Jump", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 650));
            new TextObject(content, "Shift Dash", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 700));

            var arrow = new Sprite(state, "arrow", "Default", new Rectangle(0, 0, 0, 0), new Vector2(-100, 0) + exitPosition, mainLayer, false);
            arrow.AddBehavior(new Sine(0, 10, new Vector2(1, 0), 10, true));

            state.Connect(world.Client);
            GameControls secondControls = new GameControls();
            secondControls.ChangeControl(Control.left, () => Keyboard.GetState().IsKeyDown(Keys.H));
            secondControls.ChangeControl(Control.right, () => Keyboard.GetState().IsKeyDown(Keys.K));
            secondControls.ChangeControl(Control.jump, () => Keyboard.GetState().IsKeyDown(Keys.U));
            secondControls.ChangeControl(Control.dash, () => Keyboard.GetState().IsKeyDown(Keys.J));
            secondControls.ChangeControl(Control.pause, () => Keyboard.GetState().IsKeyDown(Keys.I));
            state.Players.First().GetBehavior<TimerHandler>("TimerHandler")
                .SetTimer("spawnNew", TimeSpan.FromSeconds(3), (t) => state.Connect(new GameClient(world.Client.Window, secondControls, world.Client.Language)), true);

            return state;
        }
    }
}