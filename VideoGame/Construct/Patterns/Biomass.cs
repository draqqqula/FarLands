using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;

namespace VideoGame
{
    /// <summary>
    /// Шаблон неподвижного врага
    /// </summary>
    public class Biomass : IPattern
    {
        public string AnimatorName => "Biomass";

        public string InitialAnimation => "Default";

        public Rectangle Hitbox => new Rectangle(-55, -55, 110, 110);

        public bool IsHitBoxOnly => false;

        public List<GameObject> Editions { get; set; }

        private Family Entities;

        public GameObject InitializeMember(IGameState state, GameObject member)
        {
            member.AddBehavior(new Collider(18, true));
            member.AddBehavior(new TimerHandler(true));
            var contact = new DamageInstance(
                        new Dictionary<DamageType, int>()
                        {
                            { DamageType.Physical, 25 }
                        },
                        Team.enemy,
                        new HashSet<string>(),
                        "Contact",
                        null,
                        new List<Func<Dummy, DamageInstance, bool>>() { PatternUpdaters.CheckDashInvulnerability },
                        null,
                        TimeSpan.FromSeconds(1)
                        );
            member.AddBehavior(new DamageContainer(true, ("Contact", contact)));
            member.AddBehavior(new Sine(0, 10, new Vector2(0, 1), 6, true));
            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            member.ApplyContactDamage(Entities);
        }

        public Biomass(LocationState location, Family entities)
        {
            Entities = entities;
            Editions = new List<GameObject>();
        }
    }
}
