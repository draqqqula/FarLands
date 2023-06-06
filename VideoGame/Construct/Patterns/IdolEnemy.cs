﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace VideoGame
{
    public class IdolEnemy : IPattern
    {
        public List<GameObject> Editions { get; set; }

        public TileMap Surfaces { private get; set; }

        public string AnimatorName => "Idol";

        public string InitialAnimation => "Default";

        public Rectangle Hitbox => new Rectangle(-27, -120, 54, 243);

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
                (unit) => unit.GetBehavior<Sine>("Sine").Enabled = true,
                (unit) => 
                {
                    unit.GetBehavior<Physics>("Physics").AddVector("DashLeft", new MovementVector(new Vector2(-15, 0), -30, TimeSpan.Zero, true));
                    unit.GetBehavior<Sine>("Sine").Enabled = false;
                },
                null, null, true,
                (unit, target) => (target.Parent.Position.X <= unit.Parent.Position.X) ? 1 : 0
                );
            UnitAction dashRight = new UnitAction(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.5),
                (unit) => unit.GetBehavior<Sine>("Sine").Enabled = true,
                (unit) =>
                { 
                    unit.GetBehavior<Physics>("Physics").AddVector("DashRight", new MovementVector(new Vector2(15, 0), -30, TimeSpan.Zero, true));
                    unit.GetBehavior<Sine>("Sine").Enabled = false;
                },
                null, null, true,
                (unit, target) => (target.Parent.Position.X > unit.Parent.Position.X) ? 1 : 0
                );
            UnitAction jump = new UnitAction(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(3),
                (unit) => 
                {
                    var sine = unit.GetBehavior<Sine>("Sine");
                    sine.Enabled = true;
                    sine.ChangeDirection(new Vector2(0, 1));
                },
                (unit) =>
                {
                    unit.GetBehavior<Physics>("Physics").AddVector("Jump", new MovementVector(new Vector2(0, -30), -60, TimeSpan.Zero, true));
                    var factor = unit.GetBehavior<Unit>("Unit").Target.Parent.Position.X - unit.Position.X > 0 ? 1 : -1;
                    unit.GetBehavior<Physics>("Physics").AddVector("Dash", new MovementVector(new Vector2(factor * 5, 0), -5, TimeSpan.Zero, true));
                    unit.GetBehavior<Sine>("Sine").Enabled = false;
                    sine.ChangeDirection(new Vector2(1, 0));
                },
                null, 2, true,
                (unit, target) => (target.Parent.Layout.Bottom < unit.Parent.Layout.Bottom) ? 2 : 0,
                (unit, target) => Math.Abs(target.Parent.Position.X - unit.Parent.Position.X) < 400 ? 0 : -2
                );
            unit.AddActions(("DashLeft", dashLeft), ("DashRight", dashRight), ("Jump", jump));
            member.AddBehavior(unit);

            var contact = new DamageInstance(
                        new Dictionary<DamageType, int>()
                        {
                            { DamageType.Physical, 4 }
                        },
                        Team.enemy,
                        new HashSet<string>(),
                        "Contact",
                        member.GetBehavior<Dummy>("Dummy"),
                        new List<Func<Dummy, DamageInstance, bool>>() { (dummy, damage) => {
                            if (dummy.Parent.Behaviors.ContainsKey("Physics")) 
                                return !dummy.Parent.GetBehavior<Physics>("Physics").Vectors.ContainsKey("Dash"); 
                            else return false; } },
                        null,
                        TimeSpan.FromSeconds(1)
                        );

            member.AddBehavior(new DamageContainer(true, ("Contact", contact)));

            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            member.ApplyContactDamage();
            member.SearchTarget(400, 40);
        }

        public IdolEnemy(TileMap surfaces)
        {
            Editions = new List<GameObject>();
            Surfaces = surfaces;
        }
    }
}
