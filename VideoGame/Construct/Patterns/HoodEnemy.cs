using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace VideoGame
{
    public class HoodEnemy : IPattern
    {
        public IGameState State { get; set; }

        public TileMap Surfaces { private get; set; }

        public string AnimatorName => "Hood";

        public string InitialAnimation => "Default";

        public Rectangle Hitbox => new Rectangle(-30, -30, 60, 60);

        public List<GameObject> Editions { get; set; }

        public GameObject InitializeMember(Vector2 position, Layer layer, bool isMirrored)
        {
            var member = new GameObject(State, AnimatorName, InitialAnimation, Hitbox, position, layer, isMirrored);

            Sine sine = new Sine(0, 12, new Vector2(0, 1), 2, true);

            Collider collider = new Collider(18, true);

            Physics physics = new Physics(Surfaces.VerticalSurfaceMap, Surfaces.TileFrame.Width * (int)Surfaces.Scale.X, true);

            Dummy dummy = new Dummy(30, new Dictionary<DamageType, int>(), Team.enemy, null, null, 1, true);

            TimerHandler timerHandler = new TimerHandler(true);

            Unit unit = new Unit(timerHandler, true);

            member.AddBehaviors(sine, collider, physics, dummy, timerHandler, unit);

            UnitAction goForward = new UnitAction(
                TimeSpan.FromSeconds(0.3), TimeSpan.FromSeconds(0.3),
                (unit) => 
                {
                    var target = member.GetBehavior<Unit>("Unit").Target.Parent;
                    if (target.Position.X < member.Position.X)
                        member.IsMirrored = true;
                    else
                        member.IsMirrored = false;
                    physics.AddVector("Forward", new MovementVector(Vector2.Normalize(-member.Position + target.Position) * 5, 0, TimeSpan.FromSeconds(0.4), false));
                },
                (unit) => { },
                null, null, true,
                (unit, target) => ((target.Parent.Position - unit.Parent.Position).Length() > 300) ? 2 : 0
                );
            UnitAction shoot = new UnitAction(
                TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1),
                (unit) => { },
                (unit) =>
                {
                    
                },
                null, null, true,
                (unit, target) => 1
                );

            unit.AddActions(("GoForward", goForward), ("Shoot", shoot));
            member.AddBehavior(unit);

            var contact = new DamageInstance(
                        new Dictionary<DamageType, int>()
                        {
                            { DamageType.Fire, 3 }
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

        public void UpdateMember(GameObject member)
        {
            member.ApplyContactDamage();
            member.SearchTarget(400, 40);
        }

        public HoodEnemy(TileMap surfaces, IGameState state)
        {
            Editions = new List<GameObject>();
            Surfaces = surfaces;
            State = state;
        }
    }
}
