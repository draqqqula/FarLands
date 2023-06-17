using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// состояние уровня
    /// </summary>
    public interface IGameState
    {
        /// <summary>
        /// поочерёдно вычёркивает удалённые объекты, обновляет поведения, обновляет анимации, обновляет паттерны,
        /// производит действия предусмотренные наследным классом
        /// </summary>
        public void Update()
        {
            Camera.Update();
            ExcludeDestroyed();
            UpdateBehaviors();
            UpdateAnimations();
            UpdatePatterns();
            LocalUpdate();
        }

        /// <summary>
        /// действия, предусмотренные данным классом
        /// </summary>
        public void LocalUpdate();

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

        public void UpdateAnimations()
        {
            foreach (var sprite in AllObjects.Where(e => !e.IsHitBoxOnly))
            {
                sprite.UpdateAnimation();
            }
        }

        public void UpdateBehaviors()
        {
            foreach (var sprite in AllObjects.ToArray())
            {
                foreach (var behavior in sprite.ActiveBehaviors.Values.ToArray())
                {
                    behavior.Act();
                }
            }
        }

        public void UpdatePatterns()
        {
            foreach (var pattern in Patterns)
            {
                pattern.Update(this);
            }
        }

        public void ExcludeDestroyed()
        {
            AllObjects.RemoveAll(e => e.ToDestroy);
            Patterns.ForEach(pattern => pattern.Editions.RemoveAll(edition => edition.ToDestroy));
        }
    }
}
