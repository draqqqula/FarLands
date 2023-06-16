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
            member.AddBehavior(new TimerHandler(true));
            member.AddBehavior(new Collider(18, true));

            member.AddBehavior(new Playable(dummy,
                new TextObject("a", "heart", 0, 6f, 3f, state.Layers["LeftTopBound"], new Vector2(30, 30))
                ,
                new GameObject(state, "dash_bar", "Default", new Rectangle(0, 0, 28, 30), new Vector2(136, 85), state.Layers["RightBottomBound"], false),
                true)
                );
            member.GetBehavior<Physics>("Physics").AddVector("Gravity", new MovementVector(new Vector2(0, 10), 0, TimeSpan.Zero, true));
            
            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            var MyPhysics = member.GetBehavior<Physics>("Physics");
            GameControls controls = state.Controls;
            if (controls[Control.right])
            {
                MyPhysics.AddVector("LeftMovement", new MovementVector(new Vector2(8, 0), -100, TimeSpan.Zero, true));
                member.IsMirrored = false;
            }
            if (controls[Control.left])
            {
                MyPhysics.AddVector("RightMovement", new MovementVector(new Vector2(-8, 0), -100, TimeSpan.Zero, true));
                member.IsMirrored = true;
            }
            if (controls.OnPress(Control.jump) && MyPhysics.Faces[Side.Bottom] && !MyPhysics.Vectors.ContainsKey("Dash"))
            {
                MyPhysics.AddVector("Jump", new MovementVector(new Vector2(0, -21), -30, TimeSpan.Zero, true));
            }

            if (controls.OnPress(Control.dash) && MyPhysics.Faces[Side.Bottom] && !MyPhysics.Vectors.ContainsKey("Dash"))
            {
                MyPhysics.AddVector("Dash", new MovementVector(new Vector2(36 * member.MirrorFactor, 0), -150, TimeSpan.Zero, true));
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
                    member.SetAnimation("Dash", 0);
                    member.Animator.Stop();
                }

                else
                {
                    member.Animator.Resume();
                }

                Layer particles;
                if (MyPhysics.Vectors["Dash"].Module > 10 && state.Layers.TryGetValue("Particles", out particles))
                {
                    var MyTimerHandler = member.GetBehavior<TimerHandler>("TimerHandler");
                    if (MyTimerHandler.OnLoop("dash_effect", TimeSpan.FromSeconds(0.03), null))
                    {
                        var dashEffect = new GameObject(state, "dash", "Default", new Rectangle(35, 39, 13, 19), member.Position, particles, member.IsMirrored);
                        dashEffect.AddBehavior(new Fade(TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2), TimeSpan.Zero, true, true));
                    }
                }
            }
            else
            {
                if (MyPhysics.Faces[Side.Bottom])
                {
                    if (MyPhysics.Vectors.ContainsKey("LeftMovement") || MyPhysics.Vectors.ContainsKey("RightMovement"))
                        member.SetAnimation("Running", 0);
                    else
                        member.SetAnimation("Default", 0);
                }
                else
                {
                    MovementVector jump;
                    MovementVector fall;
                    if (
                        MyPhysics.Vectors.TryGetValue("Jump", out jump) &&
                        MyPhysics.Vectors.TryGetValue("Gravity", out fall) &&
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
