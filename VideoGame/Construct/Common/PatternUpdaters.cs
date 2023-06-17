using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct.Behaviors;

namespace VideoGame
{
    public static class PatternUpdaters
    {
        /// <summary>
        /// Наносит контактный урон при несоблюдении определённых условий.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="entities"></param>
        public static void ApplyContactDamage(this GameObject entity, Family entities)
        {
            var enities = entities.Members;

            var touched = entity.GetBehavior<Collider>("Collider")
                .GetCollisions
                (
                    enities.Select(e => e.GetBehavior<Collider>("Collider"))
                )
                .Select(e => e.Parent.GetBehavior<Dummy>("Dummy"))
            .ToArray();

            foreach (var dummy in touched)
            {
                dummy.TakeDamage(entity.GetBehavior<DamageContainer>("DamageContainer").GetDamage("Contact"));
            }
        }

        /// <summary>
        /// Ищет в поле зрения подходящего противника, чтобы сделать его своей целью.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="sightRange"></param>
        /// <param name="stepCount"></param>
        /// <param name="entities"></param>
        public static void SearchTarget(this GameObject entity, float sightRange, int stepCount, Family entities)
        {
            var unit = entity.GetBehavior<Unit>("Unit");
            var seen = entities.Members.Where(e => e.Position
            .HasLineOfSight
            (
            entity.Position,
            entity.GetBehavior<Physics>("Physics").GetMapSegment((int)Math.Min(entity.Position.X, e.Position.X) - 40, (int)Math.Max(entity.Position.X, e.Position.X) + 40),
            sightRange, stepCount
            )
            )
            .Where(e => e.GetBehavior<Dummy>("Dummy").Team != entity.GetBehavior<Dummy>("Dummy").Team).FirstOrDefault();
            if (seen != null)
                unit.SetTarget(seen.GetBehavior<Dummy>("Dummy"));
        }

        public static bool CheckDashInvulnerability(Dummy dummy, DamageInstance damage) => 
            !(dummy.Parent.GetBehavior<Physics>("Physics").Vectors.ContainsKey("Dash") &&
            dummy.Parent.GetBehavior<TimerHandler>("TimerHandler")
                .Check(string.Concat("OnStream_", dummy.Parent.IsMirrored ? "Left" : "Right")) == TimerState.Running
            );
    }
}
