using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;

namespace VideoGame
{
    /// <summary>
    /// состояние уровня
    /// </summary>
    public interface IGameState
    {
        public World World { get; set; }
        /// <summary>
        /// поочерёдно вычёркивает удалённые объекты, обновляет поведения, обновляет анимации, обновляет паттерны,
        /// производит действия предусмотренные наследным классом
        /// </summary>
        public void Update(TimeSpan deltaTime, Rectangle clientBounds)
        {
            Camera.Update(deltaTime, clientBounds);
            ExcludeDestroyed();
            UpdateBehaviors(deltaTime);
            UpdateAnimations(deltaTime);
            UpdatePatterns(deltaTime);
            LocalUpdate(deltaTime);
        }

        /// <summary>
        /// действия, предусмотренные данным классом
        /// </summary>
        public void LocalUpdate(TimeSpan deltaTime);

        /// <summary>
        /// все слои на уровне, каждый отрисовываются в порядке, в котором представлены идут
        /// </summary>
        public Dictionary<string, Layer> Layers { get; set; }
        /// <summary>
        /// все объекты на уровне
        /// </summary>
        public List<GameObject> AllObjects { get; set; }
        /// <summary>
        /// все паттерны на уровне
        /// </summary>
        public List<IPattern> Patterns { get; set; }
        /// <summary>
        /// управление, действующее на уровне
        /// </summary>
        public GameControls Controls { get; set; }
        /// <summary>
        /// камера уровня
        /// </summary>
        public GameCamera Camera { get; set; }

        private void UpdateAnimations(TimeSpan deltaTime)
        {
            foreach (var sprite in AllObjects.Where(e => !e.IsHitBoxOnly))
            {
                sprite.UpdateAnimation(deltaTime);
            }
        }

        private void UpdateBehaviors(TimeSpan deltaTime)
        {
            foreach (var sprite in AllObjects.ToArray())
            {
                foreach (var behavior in sprite.ActiveBehaviors.Values.ToArray())
                {
                    behavior.Act(deltaTime);
                }
            }
        }

        private void UpdatePatterns(TimeSpan deltaTime)
        {
            foreach (var pattern in Patterns)
            {
                pattern.Update(this);
            }
        }

        private void ExcludeDestroyed()
        {
            AllObjects.RemoveAll(e => e.ToDestroy);
            Patterns.ForEach(pattern => pattern.Editions.RemoveAll(edition => edition.ToDestroy));
        }
    }
}
