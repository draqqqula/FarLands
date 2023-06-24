using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace VideoGame
{
    /// <summary>
    /// Шаблон игрока.
    /// </summary>
    public class Player : IPattern
    {
        public string AnimatorName => "Player";

        public string InitialAnimation => "Default";

        public Rectangle Hitbox => new Rectangle(-22, -60, 44, 120);

        public TileMap Surfaces { private get; set; }

        public List<GameObject> Editions { get; set; }

        public bool IsHitBoxOnly => false;

        public GameObject InitializeMember(IGameState state, GameObject member)
        {
            member.AddBehavior(new Physics(Surfaces.VerticalSurfaceMap, Surfaces.TileFrame.Width * (int)Surfaces.Scale.X, true));
            var dummy = new Dummy(15, null, Team.player, null, null, 1, true);
            member.AddBehavior(dummy);
            var timerHandler = new TimerHandler(true);
            member.AddBehavior(timerHandler);
            member.AddBehavior(new Collider(18, true));

            member.AddBehavior(new Playable(dummy, timerHandler,
                new TextObject("a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30))
                ,
                new GameObject(state, "dash_bar", "Default", new Rectangle(0, 0, 28, 30), new Vector2(136, 85), state.Layers["RightBottomBound"], false),
                true)
                );
            member.GetBehavior<Physics>("Physics").AddVector("Gravity", new MovementVector(new Vector2(0, 9), 8, 100, TimeSpan.Zero, true));
            
            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            var myPhysics = member.GetBehavior<Physics>("Physics");
            var myPlayable = member.GetBehavior<Playable>("Playable");
            var myTimerHandler = member.GetBehavior<TimerHandler>("TimerHandler");

            Rectangle smallOffset = member.PredictLayout(new Vector2(0, 1));
            Rectangle bigOffset = member.PredictLayout(new Vector2(0, 15));
            var segment = myPhysics.GetMapSegment(smallOffset.Left, smallOffset.Right).ToArray();
            bool onGround = segment.Any(t => smallOffset.Intersects(t));
            bool nearGround = segment.Any(t => bigOffset.Intersects(t));


            GameControls controls = state.Controls;
            if (controls[Control.right])
            {
                myPhysics.AddVector("LeftMovement", new MovementVector(new Vector2(8, 0), -100, TimeSpan.Zero, true));
                member.IsMirrored = false;
            }
            if (controls[Control.left])
            {
                myPhysics.AddVector("RightMovement", new MovementVector(new Vector2(-8, 0), -100, TimeSpan.Zero, true));
                member.IsMirrored = true;
            }
            if (controls.OnPress(Control.jump) && myTimerHandler.Check("OnGround") == TimerState.Running && !myPhysics.Vectors.ContainsKey("Dash"))
            {
                myPhysics.AddVector("Jump", new MovementVector(new Vector2(0, -21), -30, TimeSpan.Zero, true));
            }

            if (myPlayable.CanDash && controls.OnPress(Control.dash) && onGround && !myPhysics.Vectors.ContainsKey("Dash"))
            {
                myPhysics.AddVector("Dash", new MovementVector(new Vector2(36 * member.MirrorFactor, 0), -150, TimeSpan.Zero, true));
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
                    member.SetAnimation("Dash", 0);
                    member.Animator.Stop();
                }

                else
                {
                    member.Animator.Resume();
                }

                Layer particles = ((LocationState)state).FrontParticlesLayer;
                if (myPhysics.Vectors["Dash"].Module > 10)
                {
                    if (myTimerHandler.OnLoop("dash_effect", TimeSpan.FromSeconds(0.03), null))
                    {
                        var dashEffect = new GameObject(state, "dash", "Default", new Rectangle(35, 39, 13, 19), member.Position, particles, member.IsMirrored);
                        dashEffect.AddBehavior(new Fade(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2), TimeSpan.Zero, true, true));
                    }
                }
            }
            else
            {
                if (onGround)
                {
                    if (myPhysics.Vectors.ContainsKey("LeftMovement") || myPhysics.Vectors.ContainsKey("RightMovement"))
                        member.SetAnimation("Running", 0);
                    else
                        member.SetAnimation("Default", 0);
                }
                else
                {
                    if (onJump && onFall &&
                        jump.Module > fall.Module)
                        member.SetAnimation("Jump", 0);
                    else
                        member.SetAnimation("Fall", 0);
                }
            }
        }

        public Player(TileMap surfaces)
        {
            Editions = new List<GameObject>();
            Surfaces = surfaces;
        }
    }
}
