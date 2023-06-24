﻿using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
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
        public bool Enabled { get; set; }

        public void Act(TimeSpan deltaTime)
        {
            foreach (var invinibilityInstance in InvincibilityFrames)
            {
                InvincibilityFrames[invinibilityInstance.Key] = invinibilityInstance.Value - deltaTime;
            }
            InvincibilityFrames = InvincibilityFrames.Where(t => t.Value > TimeSpan.Zero).ToDictionary(t => t.Key, t => t.Value);
        }

        private void SpawnParticles(IGameState state, int count, Layer layer)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                float angle = (float)-(Math.PI/4 + random.NextDouble() * Math.PI/2);
                float power = (1 + (float)random.NextDouble()) * 8;
                float decceleration = -(1 + (float)random.NextDouble()) * 24;
                TimeSpan delay = TimeSpan.FromSeconds(0.2 + random.NextDouble() * 0.1);
                SpawnParticle(state, layer, angle, power, decceleration, delay);
            }
        }

        private void SpawnParticle(IGameState state, Layer layer, float angle, float power, float decceleration, TimeSpan delay)
        {
           var damageParticle = 
                new GameObject(state,
                "damage_particle",
                "Default",
                new Rectangle(9, 9, 3, 3),
                Parent.Position,
                layer,
                false
                );
            var ParentPhysics = Parent.GetBehavior<Physics>("Physics");
            var particlePhysics = new Physics(new Rectangle[0][], 15, ParentPhysics.Enabled);
            damageParticle.AddBehavior(particlePhysics);
            particlePhysics.AddVector("Gravity", new MovementVector(new Vector2(0, 1), 4, TimeSpan.Zero, true));

            particlePhysics.AddVector(
                "Impulse", 
                new MovementVector(
                    new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * power, decceleration, TimeSpan.Zero, true));

            var particleTimer = new TimerHandler(true);
            damageParticle.AddBehavior(particleTimer);
            particleTimer.SetTimer("DelayDuration", delay, 
                (particle) =>
                {
                    var timer = particle.GetBehavior<TimerHandler>("TimerHandler");
                    timer.SetTimer("Destroy", TimeSpan.FromSeconds(0.3), (particle) => particle.Destroy(), true);
                    particle.ChangeAnimation("Fade", 0);
                }
                , true);
        }

        public bool TakeDamage(DamageInstance damage)
        {
            if (Team != damage.Team && !InvincibilityFrames.ContainsKey(damage.MainTag))
            {
                var allConditions = Conditions.Concat(damage.Conditions);
                if (allConditions.Count() == 0 || allConditions.All(condition => condition(this, damage)))
                {
                    var fullDamage = damage.ApplyResistance(Resistances).Sum(t => t.Value);
                    SpawnParticles(Parent.GameState, fullDamage, Parent.Layer);
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

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
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
