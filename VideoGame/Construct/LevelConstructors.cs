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

namespace VideoGame
{
    public static class LevelConstructors
    {
        public static GameObject CreatePlayer(Vector2 position, Layer layer, TileMap tileMap, IGameState state)
        {
            var player = new GameObject(state, "Player", "Default", new Rectangle(-22, -60, 44, 120), position, layer, false);
            player.AddBehavior(new Physics(tileMap.VerticalSurfaceMap, tileMap.TileFrame.Width * (int)tileMap.Scale.X, true));
            player.AddBehavior(new Dummy(15, null, Team.player, null, null, true));
            player.AddBehavior(new TimerHandler(true));
            player.GetBehavior<Physics>("Physics").AddVector("Gravity", new MovementVector(new Vector2(0, 10), 0, TimeSpan.Zero, true));
            return player;
        }

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

            state.Player = CreatePlayer(new Vector2(100, 100), mainLayer, state.MainTileMap, state);
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
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound, particlesLayer);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            var b = new GameObject(state, "Hood", "Default", new Rectangle(-30, -30, 60, 60), new Vector2(700, 700), mainLayer, false);

            b.AddBehavior(new Sine(0, 12, new Vector2(0, 1), 2, true));


            state.MainTileMap = CreateForestTilemap(Vector2.Zero, surfacesLayer);

            CreateSkyAndClouds(backgroundLayer, cloudsLayer);

            state.Player = CreatePlayer(new Vector2(300, 300), mainLayer, state.MainTileMap, state);
            camera.LinkTo(state.Player);
            camera.SetOuterBorders(state.MainTileMap.Frame);
            return state;
        }
    }

    public static class LevelHandlers
    {
        public static void UpdatePlayer(GameObject player, GameControls controls)
        {
            var state = Global.Variables.MainGame._world.CurrentLevel.GameState;

            var MyPhysics = player.GetBehavior<Physics>("Physics");
            if (controls[Control.right])
            {
                MyPhysics.AddVector("LeftMovement", new MovementVector(new Vector2(8, 0), -100, TimeSpan.Zero, true));
                player.IsMirrored = false;
            }
            if (controls[Control.left])
            {
                MyPhysics.AddVector("RightMovement", new MovementVector(new Vector2(-8, 0), -100, TimeSpan.Zero, true));
                player.IsMirrored = true;
            }
            if (controls.OnPress(Control.jump) && MyPhysics.Faces[Side.Bottom])
            {
                MyPhysics.AddVector("Jump", new MovementVector(new Vector2(0, -20), -30, TimeSpan.Zero, true));
            }

            if (controls.OnPress(Control.dash) && MyPhysics.Faces[Side.Bottom] && !MyPhysics.Vectors.ContainsKey("Dash"))
            {
                MyPhysics.AddVector("Dash", new MovementVector(new Vector2(36 * player.MirrorFactor, 0), -150, TimeSpan.Zero, true));
            }

            if (MyPhysics.Faces[Side.Top])
            {
                MovementVector jump;
                MovementVector fall;
                if (MyPhysics.Vectors.TryGetValue("Jump", out jump) && MyPhysics.Vectors.TryGetValue("Gravity", out fall))
                    jump.Module = fall.Module * 0.9f;
            }


            if (MyPhysics.Vectors.ContainsKey("Dash"))
            {
                if (MyPhysics.Vectors["Dash"].Module > 20)
                {
                    player.SetAnimation("Dash", 0);
                    player.Animator.Stop();
                    Layer particles;
                    if (state.Layers.TryGetValue("Particles", out particles))
                    {
                        var MyTimerHandler = player.GetBehavior<TimerHandler>("TimerHandler");
                        if (MyTimerHandler.OnLoop("dash_effect", TimeSpan.FromSeconds(0.03), null))
                        {
                            var dashEffect = new GameObject(state, "dash", "Default", new Rectangle(35, 39, 13, 19), player.Position, particles, player.IsMirrored);
                            dashEffect.AddBehavior(new Fade(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2), TimeSpan.Zero, true, true));
                        }
                    }
                }
                else
                {
                    player.Animator.Resume();
                }
            }
            else
            {
                if (MyPhysics.Faces[Side.Bottom])
                {
                    if (MyPhysics.Vectors.ContainsKey("LeftMovement") || MyPhysics.Vectors.ContainsKey("RightMovement"))
                        player.SetAnimation("Running", 0);
                    else
                        player.SetAnimation("Default", 0);
                }
                else
                {
                    MovementVector jump;
                    MovementVector fall;
                    if (
                        MyPhysics.Vectors.TryGetValue("Jump", out jump) &&
                        MyPhysics.Vectors.TryGetValue("Gravity", out fall) &&
                        jump.Module > fall.Module)
                        player.SetAnimation("Jump", 0);
                    else
                        player.SetAnimation("Fall", 0);
                }
            }
        }

    }

    public class LevelDeconstructors
    {

    }

    public interface IGameState
    {
        public void Update();
        public Dictionary<string, Layer> Layers { get; set; }
        public List<GameObject> AllObjects { get; set; }
        public GameCamera Camera { get; set; }
    }

    public class LocationState : IGameState
    {
        public Dictionary<string, Layer> Layers { get; set; }
        public List<GameObject> AllObjects { get; set; }
        public GameObject Player;
        public GameControls Controls;
        public GameCamera Camera { get; set; }
        public TileMap MainTileMap;

        public void AddLayers(params Layer[] layers)
        {
            foreach (var layer in layers)
                Layers.Add(layer.Name, layer);
            Layers = Layers.OrderBy(e => e.Value.DrawingPriority).ToDictionary(e => e.Key, e => e.Value);
        }

        public void Update()
        {
            Camera.Update();

            Global.Updates.ExcludeDestroyed();
            Global.Updates.UpdateBehaviors();
            Global.Updates.UpdateAnimations();

            LevelHandlers.UpdatePlayer(Player, Controls);
        }

        public LocationState()
        {
            Layers = new Dictionary<string, Layer>();
            AllObjects = new List<GameObject>();
        }
    }
}
