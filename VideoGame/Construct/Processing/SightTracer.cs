using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// содержит методы для работы с видимостью
    /// </summary>
    public static class SightTracer
    {
        /// <summary>
        /// true если луч между source и target не касается поверхностей
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="surfaces"></param>
        /// <param name="sightDistance"></param>
        /// <param name="stepCount"></param>
        /// <returns></returns>
        public static bool HasLineOfSight(this Vector2 source, Vector2 target, IEnumerable<Rectangle> surfaces, float sightDistance, int stepCount)
        {
            Vector2 direction = target - source;
            direction.Normalize();
            float distance = (target - source).Length();
            if (distance > sightDistance)
                return false;
            for (float l = 0; l < distance; l += distance/stepCount)
            {
                 if (surfaces.Any(s => s.Contains(source + l*direction)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
