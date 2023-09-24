using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    [NoVisuals]
    [Box(30)]
    public class Stream : Sprite
    {
        public Stream(GameState state, Rectangle hitBox, Vector2 position,
            Layer mainLayer, Layer frontLayer, Layer backLayer, Family entities, Side direction,
            double particleDenisty, double spawnProbabilitySpread)

            : base(state, position, mainLayer, false)
        {
            Direction = direction;
            Entities = entities;
            ParticleFrontLayer = frontLayer;
            ParticleBackLayer = backLayer;
            ParticleDenisty = particleDenisty;
            SpawnProbabilitySpread = spawnProbabilitySpread;
            Box = hitBox;
            AddBehavior(new TimerHandler(true));
            OnAssembled(this);
        }

        private readonly Layer ParticleFrontLayer;
        private readonly Layer ParticleBackLayer;
        private readonly Family Entities;
        private readonly Side Direction;
        private readonly double ParticleDenisty;
        private readonly double SpawnProbabilitySpread;

        public override void OnTick(GameState state, TimeSpan deltaTime)
        {
            var timerHandler = GetBehavior<TimerHandler>("TimerHandler");
            double commonParticleCount = ParticleDenisty * (Box.Width * Box.Height);
            if (state.AllCameras.Any(it => it.Sees(this)) && timerHandler.OnLoop("MakeParticle", TimeSpan.FromSeconds(0.05), null))
            {
                Random random = new Random();
                for (int i = 0; i < commonParticleCount + random.NextDouble() * SpawnProbabilitySpread; i++)
                {
                    float randomX = (float)(Layout.Left + random.NextDouble() * Box.Width);
                    float randomY = (float)(Layout.Top + random.NextDouble() * Box.Height);
                    string animation = string.Concat("Option", random.Next(1, 16));
                    MakeParticle(
                        this,
                        state,
                        new Vector2(randomX, randomY),
                        5f + (float)random.NextDouble() * 3.0f,
                        Direction,
                        TimeSpan.FromSeconds(0.2 + random.NextDouble() * 0.1),
                        animation,
                        random.NextDouble() > 0.5 ? ParticleFrontLayer : ParticleBackLayer
                        );
                }
            }

            foreach (var entityTimer in Entities
                .Where(e => e.Layout.Intersects(Layout))
                .Select(e => e.GetBehavior<TimerHandler>("TimerHandler"))
                )
            {
                entityTimer.Hold(string.Concat("OnStream_", Enum.GetName(Direction)), TimeSpan.FromSeconds(0.5), null, true);
            }
        }

        private void MakeParticle(Sprite member, GameState state, Vector2 position, float speed, Side direction, TimeSpan duration, string option, Layer layer)
        {
            var damageParticle =
                    new Sprite(
                        state,
                        "wind_particles",
                        option,
                        new Rectangle(0, 0, 0, 0),
                        position,
                        layer,
                        false
                    );
            var particlePhysics = new Physics(new Rectangle[0][], 15, true);
            damageParticle.AddBehavior(particlePhysics);
            int Xfactor = 0;
            int Yfactor = 0;
            switch (direction)
            {
                case Side.Left:
                    Xfactor = -1;
                    break;
                case Side.Right:
                    Xfactor = 1;
                    break;
                case Side.Top:
                    Yfactor = 1;
                    break;
                case Side.Bottom:
                    Yfactor = -1;
                    break;
            }
            particlePhysics.AddVector("Stream", new MovementVector(new Vector2(Xfactor * speed, Yfactor * speed), 0, TimeSpan.Zero, true));

            var fade = new Fade(TimeSpan.Zero, duration, TimeSpan.Zero, true, true);
            damageParticle.AddBehavior(fade);
        }
    }
}
