using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Condition = System.Func<VideoGame.Dummy, VideoGame.DamageInstance, bool>;
using DamageEvent = System.Func<VideoGame.Dummy, VideoGame.DamageInstance, VideoGame.DamageInstance>;

namespace VideoGame
{
    public enum Team
    {
        player,
        enemy
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
                .Select(damage => (damage.Key, Math.Max(damage.Value - resistance[damage.Key], 0)))
                .ToDictionary(t => t.Key, t => t.Item2);
        }

        public DamageInstance(Dictionary<DamageType, int> damage, Team team, HashSet<string> tags, Dummy owner, List<Condition> conditions, List<DamageEvent> events, TimeSpan invincibilityGift)
        {
            Damage = damage;
            Team = team;
            Tags = tags;
            Owner = owner;
            Conditions = conditions;
            Events = events;
            InvincibilityGift = invincibilityGift;
        }
    }
    public class Dummy : IBehavior
    {
        public string Name => "Dummy";

        public int MaxHealth { get; private set; }
        public int Health { get; private set; }
        public bool IsAlive { get { return Health > 0; } }
        public Dictionary<DamageType,int> Resistances { get; private set; }
        public Team Team { get; private set; }
        public List<Condition> Conditions { get; private set; }
        public List<DamageEvent> Events { get; private set; }
        public Dictionary<string, TimeSpan> InvincibilityFrames { get; private set; }
        public double InvincibilityFactor { get; private set; }

        public GameObject Parent { get; set; }

        public void Act()
        {
            foreach (var invinibilityInstance in InvincibilityFrames)
            {
                InvincibilityFrames[invinibilityInstance.Key] = invinibilityInstance.Value - Global.Variables.DeltaTime;
            }
            InvincibilityFrames = InvincibilityFrames.Where(t => t.Value > TimeSpan.Zero).ToDictionary(t => t.Key, t => t.Value);
        }

        public void TakeDamage(DamageInstance damage)
        {
            if (Team != damage.Team)
            {
                var allConditions = Conditions.Concat(damage.Conditions);
                if (allConditions.Count() == 0 || allConditions.All(condition => condition(this, damage)))
                {

                    Health = Math.Max(Health - damage.ApplyResistance(Resistances).Sum(t => t.Value), 0);
                    InvincibilityFrames[damage.MainTag] = damage.InvincibilityGift * InvincibilityFactor;

                    var allEvents = Events.Concat(damage.Events);
                    foreach (var damageEvent in allEvents)
                    {
                        damageEvent(this, damage);
                    }
                }
            }
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public Dummy(int maxHealth, Dictionary<DamageType, int> resistances, Team team, List<Condition> conditions, List<DamageEvent> events)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Resistances = resistances == null ? new Dictionary<DamageType, int>() : resistances;
            Events = events == null ? new List<DamageEvent>() : events;
            Team = team;
            Conditions = conditions == null ? new List<Condition>() : conditions;
            InvincibilityFrames = new Dictionary<string, TimeSpan>();
        }
    }
}
