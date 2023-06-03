using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Animations;
using Microsoft.Xna.Framework;

namespace VideoGame
{
    public class GameObject
    {
        public IPattern Pattern { get; set; }
        public Animator Animator { get; set; }
        public Rectangle HitBox { get; set; }
        public Vector2 Position { get; set; }
        public Dictionary<string, IBehavior> Behaviors { get; private set; }
        public Dictionary<string, IBehavior> ActiveBehaviors 
        { 
            get { return Behaviors.Where(e => e.Value.Enabled).ToDictionary(e => e.Key, e => e.Value); } 
        }
        public Rectangle Layout
        {
            get { return new Rectangle((int)Position.X + HitBox.X, (int)Position.Y + HitBox.Y, HitBox.Width, HitBox.Height); }
        }

        public void Destroy()
        {
            ToDestroy = true;
        }
        public bool ToDestroy { get; private set; }

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
        public Layer Layer { get; set; }
        public bool IsMirrored { get; set; }
        public int MirrorFactor
        {
            get
            {
                return IsMirrored ? -1 : 1;
            }
        }

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

        public void AddBehavior(IBehavior behavior)
        {
            Behaviors[behavior.Name] = behavior;
            behavior.Parent = this;
        }

        public void AddBehaviors(params IBehavior[] behaviors)
        {
            foreach (var behavior in behaviors)
            {
                AddBehavior(behavior);
            }
        }

        public T GetBehavior<T>(string name) where T:IBehavior
        {
            {
                return (T)Behaviors[name];
            }
        }

        public void ChangeAnimation(string animation, int initialFrame)
        {
            Animator.ChangeAnimation(DrawingParameters, animation, initialFrame);
        }

        public void SetAnimation(string animation, int initialFrame)
        {
            if (animation != Animator.Running.Name)
                ChangeAnimation(animation, initialFrame);
        }

        public void UpdateAnimation()
        {
            Animator.Update(DrawingParameters);
        }

        public GameObject(IGameState state, string animatorName, string initialAnimation, Rectangle hitBox, Vector2 position, Layer layer, bool isMirrored)
        {
            HitBox = hitBox;
            Position = position;
            Layer = layer;
            IsMirrored = isMirrored;
            Animator = new Animator(animatorName, initialAnimation);
            Behaviors = new Dictionary<string, IBehavior>();
            ToDestroy = false;
            state.AllObjects.Add(this);
        }
    }
}
