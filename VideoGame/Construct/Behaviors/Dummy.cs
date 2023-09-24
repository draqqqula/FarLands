using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Condition = System.Func<VideoGame.Dummy, VideoGame.DamageInstance, bool>;
using DamageEvent = System.Func<VideoGame.Dummy, VideoGame.DamageInstance, VideoGame.DamageInstance>;
using VideoGame.Construct.Sprites;

namespace VideoGame
{
    public enum Team
    {
        player,
        enemy,
        empty
    }
    public enum DamageType
    {
        Physical,
        Fire,
        Ice,
        Wind,
        Lightning,
        Holy
    }
    /// <summary>
    /// Содержит информацию об экземпляре урона.
    /// Если объект проходит проверки Checks, то ему наносится урон Damage,
    /// вызываются события Events,
    /// и даётся временная неуязвимость на срок InvincibilityGift от объектов с тегом урона MainTag.
    /// </summary>
    public class DamageInstance
    {
        public Dictionary<DamageType, int> Damage;
        public Team Team;
        public HashSet<string> Tags;
        public string MainTag;
        public Dummy Owner;
        public List<Condition> Conditions;
        public List<DamageEvent> Events;
        public TimeSpan InvincibilityGift;

        public Dictionary<DamageType, int> ApplyResistance(Dictionary<DamageType, int> resistance)
        {
            return Damage
                .Select(damage => (damage.Key, Math.Max(damage.Value - resistance.GetValueOrDefault(damage.Key, 0), 0)))
                .ToDictionary(t => t.Key, t => t.Item2);
        }

        public DamageInstance(Dictionary<DamageType, int> damage, Team team, HashSet<string> tags, string mainTag, Dummy owner, List<Condition> conditions, List<DamageEvent> events, TimeSpan invincibilityGift)
        {
            Damage = damage;
            Team = team;
            Tags = tags;
            MainTag = mainTag;
            Owner = owner;
            Conditions = conditions == null? new List<Condition>() : conditions;
            Events = events == null? new List<DamageEvent>() : events;
            InvincibilityGift = invincibilityGift;
        }
    }
    /// <summary>
    /// поведение объекта, которму может быть нанесён урон
    /// </summary>
    public class Dummy : Behavior
    {
        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public bool IsAlive { get { return Health > 0; } }
        public Dictionary<DamageType,int> Resistances { get; private set; }
        public Team Team { get; private set; }
        public List<Condition> Conditions { get; private set; }
        public List<DamageEvent> Events { get; private set; }
        public Dictionary<string, TimeSpan> InvincibilityFrames { get; private set; }
        public double InvincibilityFactor { get; private set; }

        public override void Act(TimeSpan deltaTime)
        {
            foreach (var invinibilityInstance in InvincibilityFrames)
            {
                InvincibilityFrames[invinibilityInstance.Key] = invinibilityInstance.Value - deltaTime;
            }
            InvincibilityFrames = InvincibilityFrames.Where(t => t.Value > TimeSpan.Zero).ToDictionary(t => t.Key, t => t.Value);
        }

        private void SpawnParticles(GameState state, int count, Layer layer)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                float angle = (float)-(Math.PI/4 + random.NextDouble() * Math.PI/2);
                float power = (2 + (float)random.NextDouble()) * 5;
                float decceleration = -(1 + (float)random.NextDouble()) * 17;
                TimeSpan delay = TimeSpan.FromSeconds(0.2 + random.NextDouble() * 0.1);
                SpawnParticle(state, layer, angle, power, decceleration, delay);
            }
        }

        private void SpawnParticle(GameState state, Layer layer, float angle, float power, float decceleration, TimeSpan delay)
        {
            new DamageParticle(state, Parent.Position, layer, angle, power, decceleration, delay);
        }

        public bool TakeDamage(DamageInstance damage)
        {
            if (Team != damage.Team && !InvincibilityFrames.ContainsKey(damage.MainTag))
            {
                var allConditions = Conditions.Concat(damage.Conditions);
                if (allConditions.Count() == 0 || allConditions.All(condition => condition(this, damage)))
                {
                    var fullDamage = damage.ApplyResistance(Resistances).Sum(t => t.Value);
                    SpawnParticles(Parent.GameState, fullDamage, Parent.PresentLayer);
                    Health = Math.Max(Health - fullDamage, 0);
                    InvincibilityFrames[damage.MainTag] = damage.InvincibilityGift * InvincibilityFactor;

                    var allEvents = Events.Concat(damage.Events);
                    foreach (var damageEvent in allEvents)
                    {
                        damageEvent(this, damage);
                    }
                    return true;
                }
            }
            return false;
        }

        public Dummy(int maxHealth, Dictionary<DamageType, int> resistances, Team team, List<Condition> conditions, List<DamageEvent> events, double invincibilityFactor, bool enabled)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Resistances = resistances == null ? new Dictionary<DamageType, int>() : resistances;
            Events = events == null ? new List<DamageEvent>() : events;
            Team = team;
            Conditions = conditions == null ? new List<Condition>() : conditions;
            InvincibilityFrames = new Dictionary<string, TimeSpan>();
            InvincibilityFactor = invincibilityFactor;
            Enabled = enabled;
        }
    }
}
