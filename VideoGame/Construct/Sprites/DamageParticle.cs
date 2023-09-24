using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace VideoGame.Construct.Sprites
{
    [Box(9,9,3,3)]
    [SpriteSheet("damage_particle")]
    public class DamageParticle : Sprite
    {
        public DamageParticle(GameState state, Vector2 position, Layer layer, float angle, float power, float decceleration, TimeSpan delay)
            : base(state, position, layer, false)
        {
            var particlePhysics = new Physics(new Rectangle[0][], 15, true);
            AddBehavior(particlePhysics);
            particlePhysics.AddVector("Gravity", new MovementVector(new Vector2(0, 1), 10, TimeSpan.Zero, true));

            particlePhysics.AddVector(
                "Impulse",
                new MovementVector(
                    new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * power, decceleration, TimeSpan.Zero, true));

            var particleTimer = new TimerHandler(true);
            AddBehavior(particleTimer);
            particleTimer.SetTimer("DelayDuration", delay,
                (particle) =>
                {
                    var timer = particle.GetBehavior<TimerHandler>("TimerHandler");
                    timer.SetTimer("Destroy", TimeSpan.FromSeconds(0.3), (particle) => particle.Dispose(), true);
                    particle.ChangeAnimation("Fade", 0);
                }
                , true);
            OnAssembled(this);
        }
    }
}
