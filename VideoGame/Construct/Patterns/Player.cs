using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoGame.Construct.Behaviors;
using VideoGame.Construct.Patterns;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace VideoGame
{
    [SpriteSheet("Player")]
    [Box(22, 60)]
    [MemberShip("Entity")]
    public class Character : Sprite
    {
        public Character(GameState state, Vector2 position, Layer layer, bool isMirrored, GameClient client, TileMap surfaces, TextObject healthBar) : 
            base(state, position, layer, isMirrored)
        {
            Client = client;
            AddBehavior(new Physics(surfaces.VerticalSurfaceMap, surfaces.TileFrame.Width * (int)surfaces.PictureScale.X, true));
            var dummy = new Dummy(15, null, Team.player, null, null, 1, true);
            AddBehavior(dummy);
            var timerHandler = new TimerHandler(true);
            AddBehavior(timerHandler);
            AddBehavior(new Collider(18, true));

            AddBehavior(new Playable(dummy, timerHandler,
                healthBar
                ,
                new DashBar(state, new Vector2(136, 85), state.Layers["RightBottomBound"], client),
                true)
                );
            GetBehavior<Physics>("Physics").AddVector("Gravity", new MovementVector(new Vector2(0, 9), 8, 100, TimeSpan.Zero, true));
            OnAssembled(this);
        }

        public readonly GameClient Client;

        public override void OnTick(GameState state, TimeSpan deltaTime)
        {
            var myPhysics = GetBehavior<Physics>();
            var myPlayable = GetBehavior<Playable>();
            var myTimerHandler = GetBehavior<TimerHandler>();

            Rectangle smallOffset = PredictLayout(new Vector2(0, 1));
            Rectangle bigOffset = PredictLayout(new Vector2(0, 15));
            var segment = myPhysics.GetMapSegment(smallOffset.Left, smallOffset.Right).ToArray();
            bool onGround = segment.Any(t => smallOffset.Intersects(t));
            bool nearGround = segment.Any(t => bigOffset.Intersects(t));
            GameControls controls = Client.Controls;


            if (controls[Control.right])
            {
                myPhysics.AddVector("LeftMovement", new MovementVector(new Vector2(8, 0), -100, TimeSpan.Zero, true));
                IsMirrored = false;
            }
            if (controls[Control.left])
            {
                myPhysics.AddVector("RightMovement", new MovementVector(new Vector2(-8, 0), -100, TimeSpan.Zero, true));
                IsMirrored = true;
            }
            if (controls.OnPress(Control.jump) && myTimerHandler.Check("OnGround") == TimerState.Running && !myPhysics.Vectors.ContainsKey("Dash"))
            {
                myPhysics.AddVector("Jump", new MovementVector(new Vector2(0, -21), -30, TimeSpan.Zero, true));
            }

            if (myPlayable.CanDash && controls.OnPress(Control.dash) && onGround && !myPhysics.Vectors.ContainsKey("Dash"))
            {
                myPhysics.AddVector("Dash", new MovementVector(new Vector2(36 * MirrorFactor, 0), -150, TimeSpan.Zero, true));
                myPlayable.UseDash();
                myTimerHandler.SetTimer("RecoverDash", TimeSpan.FromSeconds(1), (obj) => myPlayable.RecoverDash(), true);
            }

            MovementVector jump;
            MovementVector fall;
            bool onJump = myPhysics.Vectors.TryGetValue("Jump", out jump);
            bool onFall = myPhysics.Vectors.TryGetValue("Gravity", out fall);
            if (onFall)
            {
                if (onGround)
                {
                    fall.Originalize();
                    fall.Enabled = false;
                    myTimerHandler.Hold("OnGround", TimeSpan.FromSeconds(0.08), false);
                    if (onJump && jump.Module <= fall.Module)
                    {
                        myPhysics.Vectors.Remove("Jump");
                    }
                }
                else
                {
                    fall.Enabled = true;
                }
                if (onJump && myPhysics.Faces[Side.Top])
                {
                    jump.Module = fall.Module * 0.9f;
                }
            }


            if (myPhysics.Vectors.ContainsKey("Dash"))
            {
                if (myPhysics.Vectors["Dash"].Module > 20)
                {
                    SetAnimation("Dash", 0);
                    Animator.Stop();
                }

                else
                {
                    Animator.Resume();
                }

                Layer particles = ((LocationState)state).FrontParticlesLayer;
                if (myPhysics.Vectors["Dash"].Module > 10)
                {
                    if (myTimerHandler.OnLoop("dash_effect", TimeSpan.FromSeconds(0.03), null))
                    {
                        var dashEffect = new Sprite(state, "dash", "Default", new Rectangle(35, 39, 13, 19), Position, particles, IsMirrored);
                        dashEffect.AddBehavior(new Fade(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2), TimeSpan.Zero, true, true));
                    }
                }
            }
            else
            {
                if (onGround)
                {
                    if (myPhysics.Vectors.ContainsKey("LeftMovement") || myPhysics.Vectors.ContainsKey("RightMovement"))
                        SetAnimation("Running", 0);
                    else
                        SetAnimation("Default", 0);
                }
                else
                {
                    if (onJump && onFall &&
                        jump.Module > fall.Module)
                        SetAnimation("Jump", 0);
                    else
                        SetAnimation("Fall", 0);
                }
            }
        }
    }
}
