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
            return keyboard_controls;
        }

        public static void CreateSkyAndClouds(Layer skyLayer, Layer cloudsLayer)
        {
            new TileMap(Vector2.Zero, new byte[4, 4] {
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 },
                { 2, 1, 3, 3 } },
                Global.Variables.MainContent.Load<Texture2D>("Sky"),
            new Rectangle(0, 0, 396, 100), skyLayer, new Vector2(3, 3), 3);

            new TileMap(new Vector2(0, 400), new byte[4, 1] {
                { 1 },
                { 1 },
                { 1 },
                { 1 } },
                Global.Variables.MainContent.Load<Texture2D>("Clouds"),
                new Rectangle(0, 0, 439, 115), cloudsLayer, new Vector2(3, 3), 3);
        }

        public static TileMap CreateForestTilemap(Vector2 position, Layer layer)
        {
            return new TileMap(position, "level2", "rocks", new Rectangle(0, 0, 12, 12), layer, new Vector2(3, 3),
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

        public static IGameState LoadLevel1()
        {
            var state = new LocationState();
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => a - camera.LeftTopCorner, 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer interfaceLayer = new Layer("Interface", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound);


            state.MainTileMap = CreateForestTilemap(Vector2.Zero, surfacesLayer);
            CreateSkyAndClouds(backgroundLayer, cloudsLayer);

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            return state;
        }

        public static IGameState LoadLevel2()
        {
            var state = new LocationState();
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => a - camera.LeftTopCorner, 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesLayer = new Layer("Particles", a => camera.ApplyParalax(a, 1, 1), 0.1);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.HealthBar = new TextObject("a", "heart", 0, 6f, 3f, leftTopBound, new Vector2(30, 30));
            state.FPSCounter = new TextObject("a", "pixel", 0, 6f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, surfacesLayer);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer);

            Family entities = new Family("Entities");
            IPattern idolPattern = new IdolEnemy(state.MainTileMap, entities);
            IPattern hoodPattern = new HoodEnemy(state.MainTileMap, entities);
            IPattern playerPattern = new Player(state.MainTileMap);
            entities.AddPatterns(idolPattern, hoodPattern, playerPattern);
            idolPattern.CreateCopy(state, new Vector2(2400, 200), mainLayer, false);
            idolPattern.CreateCopy(state, new Vector2(3500, 400), mainLayer, false);
            hoodPattern.CreateCopy(state, new Vector2(4000, 1000), mainLayer, true);
            state.AddPatterns(idolPattern, hoodPattern, playerPattern);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 300), mainLayer, true);
            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            return state;
        }
    }

    public static class LevelHandlers
    {
    }
}
