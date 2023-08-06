using Microsoft.Xna.Framework;
using System;
using System.CodeDom;
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
        public static void ApplyContactDamage(this Sprite entity, IEnumerable<Sprite> entities)
        {
            var enities = entities;

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
        public static void SearchTarget<T>(this Sprite entity, float sightRange, int stepCount, IEnumerable<Sprite> entities) where T : Sprite
        {
            var unit = entity.GetBehavior<Unit<T>>();
            var seen = entities.Where(e => e.Position
            .HasLineOfSight
            (
            entity.Position,
            entity.GetBehavior<Physics>("Physics").GetMapSegment((int)Math.Min(entity.Position.X, e.Position.X) - 40, (int)Math.Max(entity.Position.X, e.Position.X) + 40),
            sightRange, stepCount
            )
            )
            .FirstOrDefault();
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
