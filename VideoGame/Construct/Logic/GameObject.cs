using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Microsoft.Xna.Framework;

namespace VideoGame
{
    /// <summary>
    /// игровой объект
    /// </summary>
    public class GameObject
    {
        public double TimeScale { get; set; }
        public IGameState GameState { get; set; }

        /// <summary>
        /// шаблон, который реализует этот объект
        /// </summary>
        public IPattern Pattern { get; set; }
        /// <summary>
        /// аниматор, решающий, какой кадр вывести на экран в месте существования объекта
        /// </summary>
        public Animator Animator { get; set; }
        /// <summary>
        /// если true то аниматор не выводит кадров на экран
        /// </summary>
        public readonly bool IsHitBoxOnly;
        /// <summary>
        /// прямоугольник с которым обрабатывается коллизия
        /// </summary>
        public Rectangle HitBox { get; set; }
        /// <summary>
        /// позиция объекта в мире
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// список поведений, которые перенимает данный объект
        /// </summary>
        public Dictionary<string, IBehavior> Behaviors { get; private set; }
        /// <summary>
        /// активные поведения объекта
        /// </summary>
        public Dictionary<string, IBehavior> ActiveBehaviors 
        { 
            get { return Behaviors.Where(e => e.Value.Enabled).ToDictionary(e => e.Key, e => e.Value); } 
        }
        /// <summary>
        /// представление объекта в мире
        /// </summary>
        public Rectangle Layout
        {
            get { return new Rectangle((int)Position.X + HitBox.X, (int)Position.Y + HitBox.Y, HitBox.Width, HitBox.Height); }
        }

        /// <summary>
        /// уничтожает объект, вычёркивая ссылки на него отовсюду
        /// </summary>
        public void Destroy()
        {
            ToDestroy = true;
        }
        public bool ToDestroy { get; private set; }

        /// <summary>
        /// положение объекта если тот будет перемещён на эту величину
        /// </summary>
        /// <param name="movementPrediction"></param>
        /// <returns></returns>
        public Rectangle PredictLayout(Vector2 movementPrediction)
        {
            return new 
                Rectangle(
                (int)(Position.X + movementPrediction.X) + HitBox.X, 
                (int)(Position.Y + movementPrediction.Y) + HitBox.Y, 
                HitBox.Width, 
                HitBox.Height
                );
        }
        public Vector2 Scale
        {
            get { return new Vector2(HitBox.Width, HitBox.Height); }
        }
        public Vector2 TopLeftCorner
        {
            get { return new Vector2(Layout.Left, Layout.Top); }
        }
        /// <summary>
        /// слой, на котором отрисовывается объект
        /// </summary>
        public Layer Layer { get; set; }
        /// <summary>
        /// если true, при отрисовке объект будет зеркально повёрнут
        /// </summary>
        public bool IsMirrored { get; set; }
        public int MirrorFactor
        {
            get
            {
                return IsMirrored ? -1 : 1;
            }
        }
        /// <summary>
        /// параметры, передаваемые отрисовщику
        /// могут быть модифицированы поведениями
        /// </summary>
        private DrawingParameters DrawingParameters
        {
            get
            {
                var rawParameters = new DrawingParameters
                {
                    Position = Layer.DrawingFunction(this.Position),
                    Layer = Layer,
                    Mirroring = IsMirrored ?
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally : Microsoft.Xna.Framework.Graphics.SpriteEffects.None
                };
                foreach (var behavior in ActiveBehaviors.Values)
                    rawParameters = behavior.ChangeAppearance(rawParameters);
                return rawParameters;
            }
        }

        /// <summary>
        /// заставляет объект реализовывать это поведение
        /// </summary>
        /// <param name="behavior"></param>
        public void AddBehavior(IBehavior behavior)
        {
            Behaviors[behavior.Name] = behavior;
            behavior.Parent = this;
        }

        /// <summary>
        /// заставляет объект реализовывать эти поведения
        /// </summary>
        /// <param name="behaviors"></param>
        public void AddBehaviors(params IBehavior[] behaviors)
        {
            foreach (var behavior in behaviors)
            {
                AddBehavior(behavior);
            }
        }

        /// <summary>
        /// получить поведение объекта через имя
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public T GetBehavior<T>(string name) where T:IBehavior
        {
            {
                return (T)Behaviors[name];
            }
        }

        /// <summary>
        /// переключает аниимацию и продолжает проигрывание с этого кадра
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="initialFrame"></param>
        public void ChangeAnimation(string animation, int initialFrame)
        {
            Animator.ChangeAnimation(DrawingParameters, animation, initialFrame);
        }

        /// <summary>
        /// если эта анимция не играет сейчас, переключает анимацию и продолжает проигрывание с этого кадра
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="initialFrame"></param>
        public void SetAnimation(string animation, int initialFrame)
        {
            if (animation != Animator.Running.Name)
                ChangeAnimation(animation, initialFrame);
        }

        /// <summary>
        /// проигрывает анимацию
        /// </summary>
        public void UpdateAnimation(TimeSpan deltaTime)
        {
            Animator.Update(DrawingParameters, deltaTime);
        }

        public GameObject(IGameState state, string animatorName, string initialAnimation, Rectangle hitBox, Vector2 position, Layer layer, bool isMirrored)
        {
            GameState = state;
            IsHitBoxOnly = false;
            HitBox = hitBox;
            Position = position;
            Layer = layer;
            IsMirrored = isMirrored;
            Animator = new Animator(animatorName, initialAnimation, state.MainAnimationBuilder);
            Behaviors = new Dictionary<string, IBehavior>();
            ToDestroy = false;
            state.AllObjects.Add(this);
        }

        public GameObject(IGameState state, Rectangle hitBox, Vector2 position)
        {
            GameState = state;
            IsHitBoxOnly = true;
            HitBox = hitBox;
            Position = position;
            Behaviors = new Dictionary<string, IBehavior>();
            ToDestroy = false;
            state.AllObjects.Add(this);
        }
    }
}
