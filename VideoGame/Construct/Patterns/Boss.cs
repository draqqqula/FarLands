using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;

namespace VideoGame
{
    public class Boss : IPattern
    {
            public List<GameObject> Editions { get; set; }


            private Family Entities;

            public TileMap Surfaces { private get; set; }

            public string AnimatorName => "Boss";

            public string InitialAnimation => "Default";

            public Rectangle Hitbox => new Rectangle(-54, -147, 108, 324);

            public bool IsHitBoxOnly => false;

            private readonly IPattern LeftStreamZoneMaker;
            private readonly IPattern RightStreamZoneMaker;

            public GameObject InitializeMember(IGameState state, GameObject member)
            {

                Collider collider = new Collider(18, true);
                Physics physics = new Physics(Surfaces.VerticalSurfaceMap, Surfaces.TileFrame.Width * (int)Surfaces.Scale.X, true);
                physics.AddVector("Gravity", new MovementVector(new Vector2(0, 10), 0, TimeSpan.Zero, true));
                Dummy dummy = new Dummy(30, new Dictionary<DamageType, int>(), Team.enemy, null, null, 1, true);
                TimerHandler timerHandler = new TimerHandler(true);
                Sine sine = new Sine(0, 15, new Vector2(1, 0), 15, false);
                member.AddBehavior(physics);
                member.AddBehavior(collider);
                member.AddBehavior(dummy);
                member.AddBehavior(timerHandler);
                member.AddBehavior(sine);

                Unit unit = new Unit(timerHandler, true);

                UnitAction dashLeft = new UnitAction(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.5),
                    (unit) =>
                    {
                        member.IsMirrored = false;
                        unit.GetBehavior<Sine>("Sine").Enabled = true;
                        unit.SetAnimation("Default", 0);
                    },
                    (unit, target) => true,
                    (unit) =>
                    {
                        unit.SetAnimation("Dash", 0);
                        unit.GetBehavior<Physics>("Physics").AddVector("DashLeft", new MovementVector(new Vector2(-20, 0), -30, TimeSpan.Zero, true));
                        unit.GetBehavior<Sine>("Sine").Enabled = false;
                        var zone = RightStreamZoneMaker.CreateCopy(state, member.Position);
                        zone.HitBox = member.HitBox;
                        TimerHandler timer = new TimerHandler(true);
                        Pin pin = new Pin(member, new Vector2(-65, 0), true);
                        timer.SetTimer("Destroy", TimeSpan.FromSeconds(0.3), (obj) => obj.Destroy(), true);
                        zone.AddBehaviors(timer, pin);

                    },
                    null, 2, true,
                    (unit, target) => (target.Parent.Position.X <= unit.Parent.Position.X) ? 1 : 0
                    );

                UnitAction dashRight = new UnitAction(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.5),
                    (unit) =>
                    {
                        member.IsMirrored = true;
                        unit.GetBehavior<Sine>("Sine").Enabled = true;
                        unit.SetAnimation("Default", 0);
                    },
                    (unit, target) => true,
                    (unit) =>
                    {
                        unit.SetAnimation("Dash", 0);
                        unit.GetBehavior<Physics>("Physics").AddVector("DashRight", new MovementVector(new Vector2(20, 0), -30, TimeSpan.Zero, true));
                        unit.GetBehavior<Sine>("Sine").Enabled = false;
                        var zone = LeftStreamZoneMaker.CreateCopy(state, member.Position);
                        zone.HitBox = member.HitBox;
                        TimerHandler timer = new TimerHandler(true);
                        Pin pin = new Pin(member, new Vector2(65, 0), true);
                        timer.SetTimer("Destroy", TimeSpan.FromSeconds(0.3), (obj) => obj.Destroy(), true);
                        zone.AddBehaviors(timer, pin);
                    },
                    null, 2, true,
                    (unit, target) => (target.Parent.Position.X > unit.Parent.Position.X) ? 1 : 0
                    );

                UnitAction strikeRight = new UnitAction(TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(1.3),
                    (unit) =>
                    {
                        member.IsMirrored = true;
                        unit.GetBehavior<Sine>("Sine").Enabled = true;
                        unit.SetAnimation("Default", 0);
                    },
                    (unit, target) => true,
                    (unit) =>
                    {
                        unit.SetAnimation("Strike", 0);
                        unit.GetBehavior<Physics>("Physics").AddVector("DashRight", new MovementVector(new Vector2(15, 0), -30, TimeSpan.Zero, true));
                        unit.GetBehavior<Sine>("Sine").Enabled = false;
                    },
                    null, 1, true,
                    (unit, target) => (target.Parent.Position.X > unit.Parent.Position.X) ? 0 : -2,
                    (unit, target) => Math.Abs(target.Parent.Position.X - unit.Parent.Position.X) < 400 ? 0 : 2
                    );
                UnitAction strikeLeft = new UnitAction(TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(1.3),
                    (unit) =>
                    {
                        member.IsMirrored = false;
                        unit.GetBehavior<Sine>("Sine").Enabled = true;
                        unit.SetAnimation("Default", 0);
                    },
                    (unit, target) => true,
                    (unit) =>
                    {
                        unit.SetAnimation("Strike", 0);
                        unit.GetBehavior<Physics>("Physics").AddVector("DashRight", new MovementVector(new Vector2(-15, 0), -30, TimeSpan.Zero, true));
                        unit.GetBehavior<Sine>("Sine").Enabled = false;
                    },
                    null, 1, true,
                    (unit, target) => (target.Parent.Position.X > unit.Parent.Position.X) ? -2 : 0,
                    (unit, target) => Math.Abs(target.Parent.Position.X - unit.Parent.Position.X) < 400 ? 0 : 2
                    );

                UnitAction jump = new UnitAction(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10),
                    (unit) =>
                    {
                        unit.SetAnimation("Jump", 0);
                        unit.GetBehavior<Physics>("Physics").AddVector("Jump", new MovementVector(new Vector2(0, -45), -60, TimeSpan.Zero, true));
                        var factor = unit.GetBehavior<Unit>("Unit").Target.Parent.Position.X - unit.Position.X > 0 ? 1 : -1;
                        unit.GetBehavior<Physics>("Physics").AddVector("Dash", new MovementVector(new Vector2(factor * 5, 0), -5, TimeSpan.Zero, true));
                        timerHandler.SetTimer("JumpStart", TimeSpan.FromSeconds(0.1), false);
                    },
                    (unit, target) =>
                    {
                        double progress;
                        return !unit.Parent.GetBehavior<Physics>("Physics").Faces[Side.Bottom] || timerHandler.CheckAndTurnOff("JumpStart") != TimerState.IsOut;
                    },
                    (unit) =>
                    {
                        var targetPhysics = unit.GetBehavior<Unit>("Unit").Target.Parent.GetBehavior<Physics>("Physics");
                        if (targetPhysics.Faces[Side.Bottom])
                            targetPhysics.AddVector("EarthQuake", new MovementVector(new Vector2(0, -30), -60, TimeSpan.Zero, true));

                    },
                    null, 1, true,
                    (unit, target) => Math.Abs(target.Parent.Position.X - unit.Parent.Position.X) >= 400 ? 0 : 2
                    );;
                UnitAction roar = new UnitAction(TimeSpan.FromSeconds(0.4), TimeSpan.FromSeconds(1),
                    (unit) =>
                    {
                        unit.SetAnimation("Roar", 0);
                    },
                    (unit, target) => true,
                    (unit) =>
                    {
                        unit.SetAnimation("Default", 0);
                    },
                    null, null, true,
                    (unit, target) => 1
                    );

                unit.AddActions(("DashLeft", dashLeft), ("DashRight", dashRight), ("Jump", jump), ("StrikeRight", strikeRight), ("StrikeLeft", strikeLeft), ("Roar", roar));
                member.AddBehavior(unit);

                var contact = new DamageInstance(
                            new Dictionary<DamageType, int>()
                            {
                            { DamageType.Physical, 5 }
                            },
                            Team.enemy,
                            new HashSet<string>(),
                            "Contact",
                            member.GetBehavior<Dummy>("Dummy"),
                            new List<Func<Dummy, DamageInstance, bool>>() { PatternUpdaters.CheckDashInvulnerability },
                            null,
                            TimeSpan.FromSeconds(1)
                            );

                member.AddBehavior(new DamageContainer(true, ("Contact", contact)));

                return member;
            }

            public void UpdateMember(GameObject member, IGameState state)
            {
                member.ApplyContactDamage(Entities);
                member.SearchTarget(400, 40, Entities);
            }

            public Boss(LocationState location, Family entities)
            {
                Editions = new List<GameObject>();
                Surfaces = location.MainTileMap;
                Entities = entities;
                LeftStreamZoneMaker = new StreamZone(location.BackParticlesLayer, location.FrontParticlesLayer, entities, Side.Left, 0.0003, 2);
                RightStreamZoneMaker = new StreamZone(location.BackParticlesLayer, location.FrontParticlesLayer, entities, Side.Right, 0.0003, 2);
                location.AddPatterns(LeftStreamZoneMaker, RightStreamZoneMaker);

            }
    }
}
