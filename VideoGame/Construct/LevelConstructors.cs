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

namespace VideoGame
{
    public static class LevelConstructors
    {
        public static GameObject CreatePlayer(Vector2 position, Layer layer, TileMap tileMap, IGameState state)
        {
            var player = new GameObject(state, "Player", "Default", new Rectangle(-22, -60, 44, 120), position, layer, false);
            player.AddBehavior(new Physics(tileMap.VerticalSurfaceMap, tileMap.TileFrame.Width * (int)tileMap.Scale.X));
            player.AddBehavior(new Dummy(15, null, Team.player, null, null));
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
            Layer rightBottomBound = new Layer("RightBottomBound", a => a - camera.RightBottomCorner, 1);
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound);


            state.MainTileMap = new TileMap(Vector2.Zero, "level1", "rocks", new Rectangle(0, 0, 12, 12), surfacesLayer, new Vector2(3, 3),
                new (Rectangle, Point, bool)[]
                {
                    (new Rectangle(0, 0, 12, 12), new Point(0, 0), true),
                    (new Rectangle(12, 0, 12, 12), new Point(0, 0), true),
                    (new Rectangle(24, 0, 50, 83), new Point(10, 67), false)
                });

            new TileMap(Vector2.Zero, "SkyGround", "Sky", new Rectangle(0, 0, 396, 100), backgroundLayer, new Vector2(3, 3), 3);
            new TileMap(new Vector2(0, 400), "CloudMap", "Clouds", new Rectangle(0, 0, 439, 115), cloudsLayer, new Vector2(3, 3), 3);

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
            Layer interfaceLayer = new Layer("TopLeftBound", a => a, 1);
            Layer rightBottomBound = new Layer("RightBottomBound", a => new Vector2(camera.Window.Width, camera.Window.Height) - a, 1);
            state.AddLayers(mainLayer, backgroundLayer, surfacesLayer, cloudsLayer, interfaceLayer, rightBottomBound);

            var a = new GameObject(state, "Element_Selector", "Default", new Rectangle(-11, -60, 44, 120), new Vector2(136, 85), rightBottomBound, false);
            var b = new GameObject(state, "Hood", "Default", new Rectangle(-30, -30, 60, 60), new Vector2(700, 700), mainLayer, false);

            b.AddBehavior(new Sine(0, 12, new Vector2(0, 1), 2));

            state.MainTileMap = new TileMap(Vector2.Zero, "level2", "rocks", new Rectangle(0, 0, 12, 12), surfacesLayer, new Vector2(3, 3),
                new (Rectangle, Point, bool)[]
                {
                    (new Rectangle(0, 0, 12, 12), new Point(0, 0), true),
                    (new Rectangle(12, 0, 12, 12), new Point(0, 0), true),
                    (new Rectangle(24, 0, 50, 83), new Point(10, 67), false)
                });

            new TileMap(Vector2.Zero, "SkyGround", "Sky", new Rectangle(0, 0, 396, 100), backgroundLayer, new Vector2(3, 3), 3);
            new TileMap(new Vector2(0, 400), "CloudMap", "Clouds", new Rectangle(0, 0, 439, 115), cloudsLayer, new Vector2(3, 3), 3);

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
                MyPhysics.AddVector("Dash", new MovementVector(new Vector2(36 * player.MirrorFactor, 0), -120, TimeSpan.Zero, true));
            }

            if (MyPhysics.Faces[Side.Top])
            {
                MyPhysics.RemoveVector("Jump");
            }


            if (MyPhysics.Vectors.ContainsKey("Dash"))
            {
                player.SetAnimation("Dash", 0);
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
