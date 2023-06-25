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

        public static void CreateSkyAndClouds(Layer skyLayer, Layer cloudsLayer, ContentManager content)
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

        /// <summary>
        /// Инициализирует третий уровень. На нём игрок встречается с первыми врагами.
        /// </summary>
        /// <returns></returns>
        public static IGameState LoadLevel3(World world, ContentManager content, string levelName)
        {
            Vector2 exitPosition = new Vector2(5950, 700);
            Vector2 startPosition = new Vector2(450, 200);

            var state = new LocationState();
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(content);
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => camera.ApplyParalax(a, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesFrontLayer = new Layer("ParticlesFront", a => camera.ApplyParalax(a, 1, 1), 0.1);
            Layer particlesBackLayer = new Layer("ParticlesBack", a => camera.ApplyParalax(a, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.MainLayer = mainLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level3", surfacesLayer, content);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer, content);

            Family entities = new Family("Entities");
            IPattern idolPattern = new IdolEnemy(state, entities);
            IPattern hoodPattern = new HoodEnemy(state.MainTileMap, entities);
            IPattern streamZonePattern = new StreamZone(particlesFrontLayer, particlesBackLayer, entities, Side.Left, 0.00009, 3);
            IPattern playerPattern = new Player(state.MainTileMap, new TextObject(content, "a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30)));
            entities.AddPatterns(idolPattern, hoodPattern, playerPattern);
            idolPattern.CreateCopy(state, new Vector2(1700, 700), mainLayer, false);
            idolPattern.CreateCopy(state, new Vector2(3500, 200), mainLayer, false);
            idolPattern.CreateCopy(state, new Vector2(4000, 200), mainLayer, false);
            idolPattern.CreateCopy(state, new Vector2(5200, 800), mainLayer, false);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 300), mainLayer, true);
            IPattern gatesPattern = new Gates("Level4", state.Player);
            gatesPattern.CreateCopy(state, exitPosition);

            new TextObject(content, "Enemies can also create streams", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 450));
            new TextObject(content, "when they attack", "pixel", 0, 3f, 3f, mainLayer, new Vector2(760, 500));
            new TextObject(content, "You need to catch the moment", "pixel", 0, 3f, 3f, mainLayer, new Vector2(670, 550));
            new TextObject(content, "And dash through", "pixel", 0, 3f, 3f, mainLayer, new Vector2(760, 600));

            var arrow = new GameObject(state, "arrow", "Default", new Rectangle(0, 0, 0, 0), new Vector2(-200, 0) + exitPosition, mainLayer, false);
            arrow.HitBox = new Rectangle(-10, -250, 20, 500);
            arrow.AddBehavior(new Sine(0, 10, new Vector2(1, 0), 10, true));

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            state.AddPatterns(idolPattern, hoodPattern, playerPattern, streamZonePattern, gatesPattern);
            return state;
        }

        /// <summary>
        /// Инициализирует второй уровень. На нём игрок учится пользоваться ветряным потоком.
        /// </summary>
        /// <returns></returns>
        public static IGameState LoadLevel2(World world, ContentManager content, string levelName)
        {
            var state = new LocationState();
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(content);
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => camera.ApplyParalax(a, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer bottomSurfaceLayer = new Layer("BottomSurface", a => camera.ApplyParalax(a, 1, 1), 0.15);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesFrontLayer = new Layer("ParticlesFront", a => camera.ApplyParalax(a, 1, 1), 0.16);
            Layer particlesBackLayer = new Layer("ParticlesBack", a => camera.ApplyParalax(a, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.MainLayer = mainLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, bottomSurfaceLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level2", surfacesLayer, content);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer, content);

            Family entities = new Family("Entities");
            IPattern idolPattern = new IdolEnemy(state, entities);
            IPattern hoodPattern = new HoodEnemy(state.MainTileMap, entities);
            IPattern leftStreamZonePattern = new StreamZone(particlesFrontLayer, particlesBackLayer, entities, Side.Left, 0.00001, 3);
            IPattern rightStreamZonePattern = new StreamZone(particlesFrontLayer, particlesBackLayer, entities, Side.Right, 0.00001, 3);
            IPattern biomassPattern = new Biomass(state, entities);
            IPattern playerPattern = new Player(state.MainTileMap, new TextObject(content, "a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30)));
            entities.AddPatterns(idolPattern, hoodPattern, playerPattern);

            new TextObject(content, "Dash through wind stream", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 300));
            new TextObject(content, "to avoid being hit by enemy", "pixel", 0, 3f, 3f, mainLayer, new Vector2(700, 350));

            biomassPattern.CreateCopy(state, new Vector2(750, 1000), mainLayer, false);
            var stream1 = leftStreamZonePattern.CreateCopy(state, new Vector2(1500, 1000));
            stream1.HitBox = new Rectangle(-1000, -100, 1000, 200);

            var stream2 = rightStreamZonePattern.CreateCopy(state, new Vector2(800, 1400));
            stream2.HitBox = new Rectangle(-500, -100, 500, 200);
            biomassPattern.CreateCopy(state, new Vector2(700, 1400), mainLayer, false);

            new TextObject(content, "Direction matters too", "pixel", 0, 3f, 3f, mainLayer, new Vector2(800, 1700));
            var stream3 = rightStreamZonePattern.CreateCopy(state, new Vector2(2000, 1750));
            stream3.HitBox = new Rectangle(-2000, -100, 2000, 200);
            biomassPattern.CreateCopy(state, new Vector2(700, 1750), mainLayer, false);
            biomassPattern.CreateCopy(state, new Vector2(1400, 1750), mainLayer, false);

            new TileMap(content, Vector2.Zero, "level2background", "forest_background", new Rectangle(0, 0, 12, 12), bottomSurfaceLayer, new Vector2(3, 3), 1, Color.Black);

            state.AddPatterns(idolPattern, hoodPattern, playerPattern, leftStreamZonePattern, rightStreamZonePattern, biomassPattern);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 400), mainLayer, true);

            Vector2 leftExit = new Vector2(0, 1750);
            Vector2 rightExit = new Vector2(2050, 1750);
            IPattern gatesPattern = new Gates("Level3", state.Player);

            var leftGates = gatesPattern.CreateCopy(state, leftExit);
            var leftArrow = new GameObject(state, "arrow", "Default", new Rectangle(0, 0, 0, 0), new Vector2(100, 0) + leftExit, mainLayer, true);
            leftGates.HitBox = new Rectangle(-10, -250, 20, 500);
            leftArrow.AddBehavior(new Sine(0, 10, new Vector2(1, 0), 10, true));
            var rightGates = gatesPattern.CreateCopy(state, rightExit);
            var rightArrow = new GameObject(state, "arrow", "Default", new Rectangle(0, 0, 0, 0), new Vector2(-100, 0) + rightExit, mainLayer, false);
            rightArrow.HitBox = new Rectangle(-10, -250, 20, 500);
            rightArrow.AddBehavior(new Sine(0, 10, new Vector2(1, 0), 10, true));
            state.AddPatterns(gatesPattern);

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            return state;
        }

        /// <summary>
        /// Инициализирует первый уровень. На нём игрок обучается базовым механикам передвижения
        /// </summary>
        /// <returns></returns>
        public static IGameState LoadLevel1(World world, ContentManager content, string levelName)
        {
            Vector2 exitPosition = new Vector2(5800, 1200);
            Vector2 startPosition = new Vector2(300, 300);

            var state = new LocationState();
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(content);
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => camera.ApplyParalax(a, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesFrontLayer = new Layer("ParticlesFront", a => camera.ApplyParalax(a, 1, 1), 0.1);
            Layer particlesBackLayer = new Layer("ParticlesBack", a => camera.ApplyParalax(a, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.MainLayer = mainLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level1", surfacesLayer, content);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer, content);

            Family entities = new Family("Entities");
            IPattern idolPattern = new IdolEnemy(state, entities);
            IPattern hoodPattern = new HoodEnemy(state.MainTileMap, entities);
            IPattern streamZonePattern = new StreamZone(particlesFrontLayer, particlesBackLayer, entities, Side.Left, 0.00009, 3);
            IPattern playerPattern = new Player(state.MainTileMap, new TextObject(content, "a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30)));
            entities.AddPatterns(idolPattern, hoodPattern, playerPattern);
            hoodPattern.CreateCopy(state, new Vector2(4000, 1000), mainLayer, true);
            //var stream = streamZonePattern.CreateCopy(state, new Vector2(2400, 1000));
            //stream.HitBox = new Rectangle(-60, -60, 120, 500);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 300), mainLayer, true);
            IPattern gatesPattern = new Gates("Level2", state.Player);
            gatesPattern.CreateCopy(state, exitPosition);

            new TextObject(content, "Controls", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 500));
            new TextObject(content, "A Left", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 550));
            new TextObject(content, "D Right", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 600));
            new TextObject(content, "Space Jump", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 650));
            new TextObject(content, "Shift Dash", "pixel", 0, 3f, 3f, mainLayer, new Vector2(650, 700));

            var arrow = new GameObject(state, "arrow", "Default", new Rectangle(0, 0, 0, 0), new Vector2(-100, 0) + exitPosition, mainLayer, false);
            arrow.HitBox = new Rectangle(-10, -250, 20, 500);
            arrow.AddBehavior(new Sine(0, 10, new Vector2(1, 0), 10, true));

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            state.AddPatterns(idolPattern, hoodPattern, playerPattern, streamZonePattern, gatesPattern);
            return state;
        }

        public static IGameState LoadLevel4(World world, ContentManager content, string levelName)
        {
            Vector2 startPosition = new Vector2(300, 300);

            var state = new LocationState();
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(content);
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => camera.ApplyParalax(a, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesFrontLayer = new Layer("ParticlesFront", a => camera.ApplyParalax(a, 1, 1), 0.1);
            Layer particlesBackLayer = new Layer("ParticlesBack", a => camera.ApplyParalax(a, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.MainLayer = particlesFrontLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level4", surfacesLayer, content);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer, content);

            Family entities = new Family("Entities");
            IPattern streamZonePattern = new StreamZone(particlesFrontLayer, particlesBackLayer, entities, Side.Left, 0.00009, 3);
            IPattern playerPattern = new Player(state.MainTileMap, new TextObject(content, "a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30)));
            IPattern bossPattern = new Boss(state, entities);
            entities.AddPatterns(playerPattern, bossPattern);
            bossPattern.CreateCopy(state, new Vector2(900, 200), mainLayer, true);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 300), mainLayer, true);
            IPattern gatesPattern = new Gates("Level5", state.Player);

            state.Player.GetBehavior<TimerHandler>("TimerHandler").SetTimer("BossBattle", TimeSpan.FromSeconds(25),
                (obj) =>
                {
                    gatesPattern.CreateCopy(state, state.Player.Position);
                },
                false);

            new TextObject(content, "Survive for 25 seconds", "pixel", 0, 3f, 3f, mainLayer, new Vector2(800, 200));

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            state.AddPatterns(playerPattern, streamZonePattern, bossPattern, gatesPattern);
            return state;
        }
        public static IGameState LoadLevel5(World world, ContentManager content, string levelName)
        {
            Vector2 startPosition = new Vector2(300, 300);

            var state = new LocationState();
            state.LevelLoader = new LevelLoader(world, content, levelName);
            state.MainAnimationBuilder = new AnimationBuilder(content);
            state.Controls = CreateKeyBoardControls();

            var camera = new GameCamera(new Vector2(0, 0), new Rectangle(0, 0, 600, 600));
            state.Camera = camera;
            Layer mainLayer = new Layer("Main", a => camera.ApplyParalax(a, 1, 1), 0.5);
            Layer backgroundLayer = new Layer("BackGround", a => a, 0);
            Layer surfacesLayer = new Layer("Surfaces", a => camera.ApplyParalax(a, 1, 1), 0.2);
            Layer cloudsLayer = new Layer("Clouds", a => camera.ApplyParalax(a, 0.07f, 0.03f), 0.1);
            Layer particlesFrontLayer = new Layer("ParticlesFront", a => camera.ApplyParalax(a, 1, 1), 0.1);
            Layer particlesBackLayer = new Layer("ParticlesBack", a => camera.ApplyParalax(a, 1, 1), 0.6);
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            Layer leftTopBound = new Layer("LeftTopBound", a => a, 1);
            state.MainLayer = particlesFrontLayer;
            state.FrontParticlesLayer = particlesFrontLayer;
            state.BackParticlesLayer = particlesBackLayer;
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesFrontLayer, particlesBackLayer, leftTopBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            state.FPSCounter = new TextObject(content, "a", "pixel", 0, 3f, 3f, leftTopBound, new Vector2(30, 75));

            state.MainTileMap = CreateForestTilemap(Vector2.Zero, "level4", surfacesLayer, content);

            //CreateSkyAndClouds(backgroundLayer, cloudsLayer, content);

            Family entities = new Family("Entities");
            IPattern playerPattern = new Player(state.MainTileMap, new TextObject(content, "a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30)));
            entities.AddPatterns(playerPattern);

            state.Player = playerPattern.CreateCopy(state, new Vector2(300, 300), mainLayer, true);

            new TextObject(content, "Congratulations", "pixel", 0, 3f, 3f, mainLayer, new Vector2(800, 400));
            new TextObject(content, "You Won", "pixel", 0, 3f, 3f, mainLayer, new Vector2(900, 450));
            new TextObject(content, "Thanks for playing", "pixel", 0, 3f, 3f, mainLayer, new Vector2(760, 500));

            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            state.AddPatterns(playerPattern);
            return state;
        }
    }
}