using Animations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGame.Construct;

namespace VideoGame
{
    public class LevelLoader
    {
        private Level PauseHandler;
        private string ThisLevel;
        private World World;
        private ContentManager Content;

        public bool IsReadyToResume
        {
            get => PauseHandler is null || !World.IsLevelActive(PauseHandler);
        }
        public Level LoadLevel(string name)
        {
            return World.LoadLevel(name, Content);
        }
        public void Pass(string name)
        {
            World.Pass(ThisLevel, name, Content);
        }
        public void RestartLevel()
        {
            World.LoadLevel(ThisLevel, Content);
        }

        public void Pause(Level pauseHandler)
        {
            World.PauseLevel(ThisLevel);
            PauseHandler = pauseHandler;
        }

        public void Resume()
        {
            World.ResumeLevel(ThisLevel);
        }

        public void Unload()
        {
            World.UnloadLevel(ThisLevel);
        }

        public LevelLoader(World world, ContentManager content, string thisLevel)
        {
            World = world;
            Content = content;
            ThisLevel = thisLevel;
        }
    }

    /// <summary>
    /// состояние уровня
    /// </summary>
    public interface IGameState
    {
        public AnimationBuilder MainAnimationBuilder { get; set; }
        public LevelLoader LevelLoader { get; set; }
        /// <summary>
        /// поочерёдно вычёркивает удалённые объекты, обновляет поведения, обновляет анимации, обновляет паттерны,
        /// производит действия предусмотренные наследным классом
        /// </summary>
        public void Update(TimeSpan deltaTime, bool paused)
        {
            if (paused)
            {
                Camera.Update(TimeSpan.Zero);
                UpdateAnimations(TimeSpan.Zero);
                if (LevelLoader.IsReadyToResume)
                    LevelLoader.Resume();
            }
            else
            {
                Camera.Update(deltaTime);
                ExcludeDestroyed();
                UpdateBehaviors(deltaTime);
                UpdateAnimations(deltaTime);
                UpdatePatterns(deltaTime);
                LocalUpdate(deltaTime);
            }
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
