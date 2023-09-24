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
using Vector3 = Microsoft.Xna.Framework.Vector3;

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

        public bool OnJump { get; private set; }
        public bool OnDash { get; private set; }
        public bool OnFall { get; private set; }
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
            MovementVector dash;
            OnJump = myPhysics.Vectors.TryGetValue("Jump", out jump);
            OnFall = myPhysics.Vectors.TryGetValue("Gravity", out fall);
            OnDash = myPhysics.Vectors.TryGetValue("Dash", out dash);
            if (OnFall)
            {
                if (onGround)
                {
                    fall.Originalize();
                    fall.Enabled = false;
                    myTimerHandler.Hold("OnGround", TimeSpan.FromSeconds(0.08), false);
                    if (OnJump && jump.Module <= fall.Module)
                    {
                        myPhysics.Vectors.Remove("Jump");
                    }
                }
                else
                {
                    fall.Enabled = true;
                }
                if (OnJump && myPhysics.Faces[Side.Top])
                {
                    jump.Module = fall.Module * 0.9f;
                }
            }


            if (OnDash)
            {
                if (dash.Module > 20)
                {
                    SetAnimation("Dash", 0);
                    Animator.Stop();
                }

                else
                {
                    Animator.Resume();
                }

                Layer particles = ((LocationState)state).FrontParticlesLayer;
                if (dash.Module > 10)
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
                    if (OnJump && OnFall &&
                        jump.Module > fall.Module)
                        SetAnimation("Jump", 0);
                    else
                        SetAnimation("Fall", 0);
                }
            }
            ManageWeapon(deltaTime);
        }

        #region WEAPON_MANAGEMENT
        public PrerenderedModel Weapon { get; private set; }

        public void PickUpWeapon(PrerenderedModel weapon)
        {
            Weapon = weapon;
        }

        public void DiscardWeapon()
        {
            Weapon = null;
        }

        private void ManageWeapon(TimeSpan deltaTime)
        {
            if (Weapon is null)
                return;
            Weapon.Set(new Vector2(
                MathEx.Lerp(Weapon.Position.X, -MirrorFactor * WeaponDistanceX + Position.X, LerpFactor, deltaTime),
                MathEx.Lerp(Weapon.Position.Y, Position.Y + WeaponDistanceY, LerpFactor, deltaTime)
                ));

            if (MirroredBefore != IsMirrored)
            {
                MirrorTimer = MirrorTimerRecharge;
            }

            if (!OnDash)
            {
                SideMomentum = Weapon.Rotation.Y.RotateTowards(MirrorFactor * (IdleSideTilt + IdleSideAdditionalTilt * MathF.Pow((float)(MirrorTimer / MirrorTimerRecharge), 4f)), MathF.PI * 2, IdleSideRotationAcceleration, deltaTime);

                Weapon.Rotation += new Vector3(IdleTiltX - Weapon.Rotation.X, SideMomentum - Weapon.Rotation.Y, IdleRotationZ - Weapon.Rotation.Z);
            }
            else
            {
                Weapon.Rotation = new Vector3(
                    Weapon.Rotation.X.RotateTowards(DashRotationX, MathF.PI * 2, DashRotationSpeed, deltaTime),
                    Weapon.Rotation.Y.RotateTowards(MirrorFactor * DashRotationY, MathF.PI * 2, DashRotationSpeed, deltaTime),
                    Weapon.Rotation.Z.RotateTowards(DashRotationZ, MathF.PI * 2, DashRotationSpeed, deltaTime)
                    );
            }


            MirroredBefore = IsMirrored;
            MirrorTimer = Math.Max(MirrorTimer-deltaTime.TotalSeconds, 0);

        }

        private float LerpFactor
        {
            get
            {
                if (OnDash)
                    return DashLerpFactor;
                else
                    return IdleLerpFactor;
            }
        }

        private float WeaponDistanceX
        {
            get
            {
                if (OnDash)
                    return DashDistanceX;
                else
                    return IdleDistanceX;
            }
        }

        private float WeaponDistanceY
        {
            get
            {
                if (OnDash)
                    return DashDistanceY;
                else
                    return IdleDistanceY;
            }
        }

        private const float IdleRotationX = 0f;
        private const float IdleRotationY = 0f;
        private const float IdleRotationZ = 0f;

        private const float DashRotationX = 0f;
        private const float DashRotationY = 0f;
        private const float DashRotationZ = MathF.PI/2 - MathF.PI/12;
        private const float DashRotationSpeed = MathF.PI*3;

        private const float DashLerpFactor = 0.4f;
        private const float IdleLerpFactor = 0.1f;
        private double MirrorTimerRecharge = 3;
        private double MirrorTimer = 0;
        private bool MirroredBefore = false;
        private bool ReachedCorrectSide = false;
        private float SideMomentum;

        private const float DashDistanceY = 0f;
        private const float IdleDistanceY = -36f;
        private const float DashDistanceX = -30f;
        private const float IdleDistanceX = 60f;
        private const float IdleTiltX = MathF.PI / 10;
        private const float WalkTiltX = MathF.PI / 3;
        private const float IdleRotationSpeedX = MathF.PI / 12;
        private const float IdleSideRotationAcceleration = MathF.PI * 2;
        private const float IdleSideTilt = MathF.PI / 6;
        private const float IdleSideAdditionalTilt = MathF.PI / 6;
        private const float SideMomentumFadeFactor = 0.89f;
        private const float SideMomentumRecoverSpeed = MathF.PI / 6;
        private const float SideMaxMomentumTilt = MathF.PI / 6;
        #endregion
    }
}