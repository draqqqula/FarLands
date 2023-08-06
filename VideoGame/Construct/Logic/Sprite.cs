using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Animations;
using Microsoft.Xna.Framework;
using VideoGame.Construct;
using static System.Windows.Forms.AxHost;

namespace VideoGame
{
    /// <summary>
    /// игровой объект
    /// </summary>
    public class Sprite : GameObject
    {
        #region FAMILIY RELATED

        private HashSet<Type> Memberships = new HashSet<Type>();
        public bool IsMember<T>()
        {
            return Memberships.Contains(typeof(T));
        }
        #endregion

        #region REPRESENTATION
        public double TimeScale { get; set; }
        #endregion

        #region APPEARANCE
        public override IDisplayable GetVisualPart(GameCamera camera)
        {
            return Animator.GetVisual(PresentLayer.DrawingFunction(DisplayInfo, camera));
        }

        public readonly Animator Animator;

        public readonly bool IsHitBoxOnly;
        public GameState GameState { get; set; }
        public bool IsMirrored { get; set; }
        public int MirrorFactor
        {
            get
            {
                return IsMirrored ? -1 : 1;
            }
        }
        protected override DrawingParameters DisplayInfo
        {
            get
            {
                var rawParameters = new DrawingParameters
                {
                    Position = this.Position,
                    Mirroring = IsMirrored ?
                    Microsoft.Xna.Framework.Graphics.SpriteEffects.FlipHorizontally : Microsoft.Xna.Framework.Graphics.SpriteEffects.None
                };
                foreach (var behavior in ActiveBehaviors.Values)
                    rawParameters = behavior.ChangeAppearance(rawParameters);
                return rawParameters;
            }
        }

        public void ChangeAnimation(string animation, int initialFrame)
        {
            Animator.ChangeAnimation(animation, initialFrame);
        }

        public void SetAnimation(string animation, int initialFrame)
        {
            if (animation != Animator.Running.Name)
                ChangeAnimation(animation, initialFrame);
        }

        public void UpdateAnimation(TimeSpan deltaTime)
        {
            Animator.Update(deltaTime);
        }

        public void TurnTo(Vector2 position, bool isRightDirectionDefault)
        {
            if (isRightDirectionDefault ^ (Position.X > position.X))
                IsMirrored = false;
            else
                IsMirrored = true;
        }

        #endregion

        #region BEHAVIOR
        public Dictionary<string, Behavior> Behaviors { get; private set; }

        public Dictionary<string, Behavior> ActiveBehaviors
        {
            get { return Behaviors.Where(e => e.Value.Enabled).ToDictionary(e => e.Key, e => e.Value); }
        }


        public void AddBehavior(Behavior behavior)
        {
            AddBehavior(behavior.GetType().Name, behavior);
        }

        public void AddBehavior(string name, Behavior behavior)
        {
            Behaviors[name] = behavior;
            behavior.Parent = this;
        }

        public void AddBehaviors(params Behavior[] behaviors)
        {
            foreach (var behavior in behaviors)
            {
                AddBehavior(behavior);
            }
        }

        public T GetBehavior<T>(string name) where T : Behavior
        {
            return (T)Behaviors[name];
        }

        public T GetBehavior<T>() where T : Behavior
        {
            return (T)Behaviors[typeof(T).Name];
        }

        #endregion

        #region CONSTRUCTORS
        public Sprite(GameState state, string animatorName, string initialAnimation, Rectangle hitBox, Vector2 position, Layer layer, bool isMirrored)
        {
            GameState = state;
            IsHitBoxOnly = false;
            Box = hitBox;
            Position = position;
            PresentLayer = layer;
            IsMirrored = isMirrored;
            Animator = new Animator(animatorName, initialAnimation, state.MainAnimationBuilder);
            Behaviors = new Dictionary<string, Behavior>();
            TimeScale = 1;
            state.AllObjects.Add(this);
        }
        public Sprite(GameState state, Rectangle hitBox, Vector2 position)
        {
            GameState = state;
            IsHitBoxOnly = true;
            Box = hitBox;
            Position = position;
            Behaviors = new Dictionary<string, Behavior>();
            TimeScale = 1;
            state.AllObjects.Add(this);
        }

        protected Action<Sprite> OnAssembled = (obj) => { };

        protected Sprite(GameState state, Vector2 position, Layer layer, bool isMirrored) : base()
        {
            GameState = state;
            IsHitBoxOnly = false;
            Position = position;
            PresentLayer = layer;
            IsMirrored = isMirrored;

            Animator animator = null;
            bool isHBonly = true;
            Rectangle hitbox = new Rectangle(-6, -6, 12, 12);
            ParseAttributes(GetType().GetCustomAttributes(true), ref animator, ref isHBonly, ref hitbox, state);
            Animator = animator;
            IsHitBoxOnly = isHBonly;
            Box = hitbox;

            Behaviors = new Dictionary<string, Behavior>();
            TimeScale = 1;
            state.AllObjects.Add(this);
            layer.Add(this);
        }

        private void ParseAttributes(object[] attributes, ref Animator animator, ref bool isHBonly, ref Rectangle hitbox, GameState state)
        {
            foreach (var attribute in attributes)
            {
                if (attribute is SpriteSheetAttribute sheet)
                {
                    animator = new Animator(sheet.FileName, sheet.InitialAnimation, state.MainAnimationBuilder);
                    isHBonly = false;
                }
                else if (attribute is NoVisualsAttribute HBonly)
                {
                    isHBonly = true;
                    animator = null;
                }
                else if (attribute is BoxAttribute hb)
                {
                    hitbox = hb.Rectangle;
                }
                else if (attribute is MemberShipAttribute member)
                {
                    Type type = Family.AllFamilies[member.FamilyName];
                    Family family = state.GetFamily(member.FamilyName);
                    family.Initialize(this);

                    OnAssembled += family.AddMember;

                    Memberships.Add(type);
                    ParseAttributes(type.GetCustomAttributes(true), ref animator, ref isHBonly, ref hitbox, state);
                }
            }
        }
        #endregion

        #region ABSTRACTION
        public virtual void OnTick(GameState state, TimeSpan deltaTime)
        {
        }

        public void Update(GameState state, TimeSpan deltaTime)
        {
            OnTick(state, deltaTime);
            if (!IsHitBoxOnly)
                UpdateAnimation(deltaTime);
        }
        #endregion
    }

    public class SpriteSheetAttribute : Attribute
    {
        public string FileName { get; }
        public string InitialAnimation { get; }
        public SpriteSheetAttribute(string fileName)
        {
            FileName = fileName;
            InitialAnimation = "Default";
        }

        public SpriteSheetAttribute(string fileName, string initialAnimation) : this(fileName)
        {
            InitialAnimation = initialAnimation;
        }
    }

    public class MemberShipAttribute : Attribute
    {
        public string FamilyName { get; }
        public MemberShipAttribute(string familyName)
        {
            FamilyName = familyName;
        }
    }
}
