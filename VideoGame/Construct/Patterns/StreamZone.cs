using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class StreamZone : IPattern
    {
        public string AnimatorName => null;

        public string InitialAnimation => null;

        public Rectangle Hitbox => new Rectangle(-150, -150, 300, 300);

        public List<GameObject> Editions { get; set; }

        public bool IsHitBoxOnly => true;

        private readonly Layer ParticleLayer;
        private readonly Family Entities;
        private readonly Side Direction;

        public GameObject InitializeMember(IGameState state, GameObject member)
        {
            member.AddBehavior(new TimerHandler(true));
            return member;
        }

        public void UpdateMember(GameObject member, IGameState state)
        {
            var timerHandler = member.GetBehavior<TimerHandler>("TimerHandler");
            if (timerHandler.OnLoop("MakeParticle", TimeSpan.FromSeconds(0.05), null))
            {
                Random random = new Random();
                for (int i = 0; i < random.Next(1, 4); i++)
                {
                    float randomX = (float)(member.Layout.Left + random.NextDouble() * member.HitBox.Width);
                    float randomY = (float)(member.Layout.Top + random.NextDouble() * member.HitBox.Height);
                    string animation = string.Concat("Option", random.Next(1, 16));
                    MakeParticle(member, state, new Vector2(randomX, randomY), 5f + (float)random.NextDouble() * 3.0f, Direction, TimeSpan.FromSeconds(0.2 + random.NextDouble() * 0.1), animation);
                }
            }

            foreach (var entityTimer in Entities.Members
                .Where(e => e.Layout.Intersects(member.Layout))
                .Select(e => e.GetBehavior<TimerHandler>("TimerHandler"))
                )
            {
                entityTimer.Hold(string.Concat("OnStream_", Enum.GetName(Direction)), TimeSpan.FromSeconds(0.1), null, true);
            }
        }

        public void MakeParticle(GameObject member, IGameState state, Vector2 position, float speed, Side direction, TimeSpan duration, string option)
        {   
            var damageParticle =
                    new GameObject(
                        state,
                        "wind_particles",
                        option,
                        new Rectangle(0, 0, 0, 0),
                        position,
                        ParticleLayer,
                        false
                    );
            var particlePhysics = new Physics(new Rectangle[0][], 15, true);
            damageParticle.AddBehavior(particlePhysics);
            int Xfactor = 0;
            int Yfactor = 0;
            switch (direction)
            {
                case Side.Left:
                    Xfactor = 1;
                    break;
                case Side.Right:
                    Xfactor = -1;
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

        public StreamZone(Layer layer, Family entities, Side direction)
        {
            Direction = direction;
            Editions = new List<GameObject>();
            Entities = entities;
            ParticleLayer = layer;
        }
    }
}
