using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;
using VideoGame.Construct.Families;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace VideoGame
{
    #region UNITMOVES
    [EndlessMove]
    [MoveFreeSpan(0.2)]
    public class HoodGoForward : UnitMove<Hood>
    {
        public override int GetAttraction(Hood unit, Dummy target)
        {
             return 1;
        }
        public override void OnStart(Hood unit, Dummy target)
        {
            var physics = unit.GetBehavior<Physics>();
            physics.AddVector("Forward", new MovementVector(-Vector2.Normalize(unit.Position - target.Parent.Position) * 5, 0, TimeSpan.Zero, true));
            unit.SetAnimation("Forward", 0);
            unit.TurnTo(target.Parent.Position, true);
        }
        public override bool Continue(Hood unit, Dummy target)
        {
            var physics = unit.GetBehavior<Physics>();
            physics.DirectVector("Forward", target.Parent.Position - unit.Position);
            if (Vector2.Distance(unit.Position, target.Parent.Position) > 300)
            {
                return true;
            }
            return false;
        }

        public override void OnEnd(Hood unit, Dummy target)
        {
            var physics = unit.GetBehavior<Physics>();
            physics.RemoveVector("Forward");
            unit.SetAnimation("Default", 0);
            unit.TurnTo(target.Parent.Position, true);
        }

        public override void OnForcedBreak(Hood unit, Dummy target, UnitMove<Hood> breaker)
        {
            OnEnd(unit, target);
        }
    }

    [MoveDuration(1)]
    [MoveFreeSpan(0.2)]
    [MovePriority(2)]
    public class HoodFireStreamAttack : UnitMove<Hood>
    {
        public override int GetAttraction(Hood unit, Dummy target)
        {
            if (Vector2.Distance(unit.Position, target.Parent.Position) > 300)
            {
                return 0;
            }
            return 2;
        }

        public override void OnStart(Hood unit, Dummy target)
        {
            
        }

        public override void OnEnd(Hood unit, Dummy target)
        {

        }

        public override bool Continue(Hood unit, Dummy target)
        {
            return true;
        }
    }

    [EndlessMove]
    [MoveFreeSpan(0.2)]
    [MovePriority(3)]
    public class HoodDashBackwardsMove : UnitMove<Hood>
    {
        public override int GetAttraction(Hood unit, Dummy target)
        {
            return -1;
        }

        public override void OnStart(Hood unit, Dummy target)
        {
            var physics = unit.GetBehavior<Physics>();
            unit.TurnTo(target.Parent.Position, true);
            physics.AddVector("Backward", new MovementVector(-Vector2.Normalize(unit.Position - target.Parent.Position) * -5, -5, TimeSpan.Zero, true));
        }

        public override bool Continue(Hood unit, Dummy target)
        {
            var physics = unit.GetBehavior<Physics>();
            return physics.ActiveVectors.ContainsKey("Backward");
        }
    }
    #endregion

    [SpriteSheet("Hood")]
    [Box(30)]
    [MemberShip("Entity")]
    public class Hood : Sprite
    {
        public TileMap Surfaces { private get; set; }
        private Entity Entities;
        public Hood(GameState state, Vector2 position, Layer layer, bool isMirrored, TileMap surfaces, Entity entities) :
            base(state, position, layer, isMirrored)
        {
            Surfaces = surfaces;
            Entities = entities;
            Sine sine = new Sine(0, 12, new Vector2(0, 1), 2, true);

            Collider collider = new Collider(18, true);

            Physics physics = new Physics(Surfaces.VerticalSurfaceMap, Surfaces.TileFrame.Width * (int)Surfaces.PictureScale.X, true);

            Dummy dummy = new Dummy(30, new Dictionary<DamageType, int>(), Team.enemy, null, null, 1, true);

            TimerHandler timerHandler = new TimerHandler(true);

            Unit<Hood> unit = new Unit<Hood>(timerHandler, true,  new HoodGoForward(), new HoodFireStreamAttack(), new HoodDashBackwardsMove());

            AddBehaviors(sine, collider, physics, dummy, timerHandler, unit);

            AddBehavior(unit);

            var contact = new DamageInstance(
                        new Dictionary<DamageType, int>()
                        {
                            { DamageType.Fire, 3 }
                        },
                        Team.enemy,
                        new HashSet<string>(),
            "Contact",
                        GetBehavior<Dummy>("Dummy"),
                        new List<Func<Dummy, DamageInstance, bool>>() { (dummy, damage) => {
                            if (dummy.Parent.Behaviors.ContainsKey("Physics"))
                                return !dummy.Parent.GetBehavior<Physics>("Physics").Vectors.ContainsKey("Dash");
                            else return false; } },
                        null,
                        TimeSpan.FromSeconds(1)
            );

            AddBehavior(new DamageContainer(true, ("Contact", contact)));

            OnAssembled(this);
        }

        public override void OnTick(GameState state, TimeSpan deltaTime)
        {
            var unit = GetBehavior<Unit<Hood>>();
            if (unit.HasTarget && Math.Abs(Position.X-unit.Target.Parent.Position.X) < 40)
            {
                unit.ReactWith<HoodDashBackwardsMove>();
            }
            this.ApplyContactDamage(Entities.GetFoes(this));
            this.SearchTarget<Hood>(400, 40, Entities.GetFoes(this));
        }
    }
}
