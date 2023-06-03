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
        public static void ApplyContactDamage(this GameObject entity)
        {
            var enities = Global.Variables.MainGame._world.CurrentLevel.GameState.AllObjects
                    .Where(e => e.Behaviors.ContainsKey("Collider") && e.Behaviors.ContainsKey("Dummy") && e != entity);

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

        public static void SearchTarget(this GameObject entity, float sightRange, int stepCount)
        {
            var enities = Global.Variables.MainGame._world.CurrentLevel.GameState.AllObjects
                    .Where(e => e.Behaviors.ContainsKey("Collider") && e.Behaviors.ContainsKey("Dummy") && e != entity);
            var unit = entity.GetBehavior<Unit>("Unit");
            var seen = enities.Where(e => e.Position
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
    }
}
