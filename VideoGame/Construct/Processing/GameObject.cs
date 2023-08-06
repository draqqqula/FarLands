
using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public abstract class GameObject : IDisposable, ICloneable
    {
        #region PRESENCE
        public Layer PresentLayer { get; protected set; }
        public virtual Vector2 Position { get; protected set; }
        public virtual Rectangle Box { get; protected set; }
        public Vector2 Scale
        {
            get { return new Vector2(Box.Width, Box.Height); }
        }
        public Rectangle Layout
        {
            get => new Rectangle(Box.Location + Position.ToPoint(), Box.Size);
        }

        public Rectangle PredictLayout(Vector2 movementPrediction)
        {
            return new
                Rectangle(
                (int)(Position.X + movementPrediction.X) + Box.X,
                (int)(Position.Y + movementPrediction.Y) + Box.Y,
                Box.Width,
                Box.Height
                );
        }

        virtual public void Move(Vector2 vector)
        {
            Position += vector;
        }

        virtual public void Set(Vector2 position)
        {
            Position = position;
        }
        #endregion

        #region DRAWING
        protected virtual DrawingParameters DisplayInfo
        {
            get
            {
                return new DrawingParameters() { Position = this.Position };
            }
        }
        public abstract IDisplayable GetVisualPart(GameCamera camera);

        public readonly bool HasVisual;
        #endregion

        #region IDISPOSABLE
        public virtual void Dispose()
        {
            Disposed = true;
        }
        public bool Disposed { get; private set; } = false;
        #endregion

        #region ICLONEABLE

        private readonly bool Cloneable;
        public object Clone()
        {
            if (Cloneable)
            {
                var clone = GetType().GetConstructor(new Type[0]).Invoke(new object[0]);

                foreach (var property in GetType().GetRuntimeProperties())
                {
                    if (property.GetCustomAttribute(typeof(CloneableAttribute)) is not null)
                    {
                        var cloneable = property.GetValue(this) as ICloneable;
                        var cloned = cloneable.Clone();
                        property.SetValue(clone, cloned);
                    }
                    else if (property.GetCustomAttribute(typeof(CustomCloningAttribute)) is not null)
                    {
                        var cloner = GetType().GetMethod("Get" + property.Name + "Clone");
                        var cloned = cloner.Invoke(this, new object[] { property.GetValue(this) });
                        property.SetValue(clone, cloned);
                    }
                    else
                    {
                        property.SetValue(clone, property.GetValue(this));
                    }
                }

                foreach (var property in GetType().GetRuntimeFields())
                {
                    if (property.GetCustomAttribute(typeof(CloneableAttribute)) is not null)
                    {
                        var cloneable = property.GetValue(this) as ICloneable;
                        var cloned = cloneable.Clone();
                        property.SetValue(clone, cloned);
                    }
                    else if (property.GetCustomAttribute(typeof(CustomCloningAttribute)) is not null)
                    {
                        var cloner = GetType().GetMethod("Get" + property.Name + "Clone");
                        var cloned = cloner.Invoke(this, new object[] { property.GetValue(this) });
                        property.SetValue(clone, cloned);
                    }
                    else
                    {
                        property.SetValue(clone, property.GetValue(this));
                    }
                }

                return clone;
            }
            else
            {
                throw new Exception("Object does not support cloning");
            }
        }

        #endregion

        #region VISIBILITY

        public bool IsVisible = true; 
        private readonly HashSet<GameClient> ClientList = new HashSet<GameClient>();
        private readonly bool ReversedVisibility;

        public void AddClient(GameClient client)
        {
            ClientList.Add(client);
        }

        /// <summary>
        /// Если не включена обратная видимость, то объект видим для всех кроме клиентов из списка. Если включена обратная видимость то объект видим только для клиентов из списка.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool IsVisibleFor(GameClient client)
        {
            return ReversedVisibility ^ !ClientList.Contains(client);
        }
        #endregion

        #region LAYER ORDER

        public void PlaceAbove(GameObject obj)
        {
            var layer = obj.PresentLayer;
            PresentLayer.Remove(this);
            layer.Insert(layer.IndexOf(obj)+1, this);
        }

        public void PlaceBelow(GameObject obj)
        {
            var layer = obj.PresentLayer;
            PresentLayer.Remove(this);
            layer.Insert(layer.IndexOf(obj), this);
        }

        public void PlaceTop()
        {
            PresentLayer.Remove(this);
            PresentLayer.Add(this);
        }

        public void PlaceBottom()
        {
            PresentLayer.Remove(this);
            PresentLayer.Insert(0, this);
        }

        #endregion

        #region CONSTRUCTORS

        public GameObject()
        {
            bool hasVisual = true;
            bool reversedVisibility = false;
            bool cloneable = false;
            Rectangle hitbox = new Rectangle(-64, -64, 128, 128);

            ParseAttributes(this.GetType().GetCustomAttributes(true), ref hasVisual, ref hitbox, ref reversedVisibility, ref cloneable);

            HasVisual = hasVisual;
            Box = hitbox;
            ReversedVisibility = reversedVisibility;
            Cloneable = cloneable;
        }
        private void ParseAttributes(object[] attributes, ref bool hasVisual, ref Rectangle hitbox, ref bool reversedVisibility, ref bool cloneable)
        {
            foreach (var attribute in attributes)
            {
                if (attribute is NoVisualsAttribute)
                {
                    hasVisual = false;
                }
                else if (attribute is BoxAttribute hb)
                {
                    hitbox = hb.Rectangle;
                }
                else if (attribute is ReversedVisibilityAttribute)
                {
                    reversedVisibility = true;
                }
                else if (attribute is CloneableAttribute)
                {
                    cloneable = true;
                }
            }
        }

        #endregion
    }

    public class CloneableAttribute : Attribute
    {
    }

    public class CustomCloningAttribute : Attribute
    {
    }

    public class ReversedVisibilityAttribute : Attribute
    {
    }

    public class NoVisualsAttribute : Attribute
    {
    }

    public class BoxAttribute : Attribute
    {
        public Rectangle Rectangle { get; }

        public BoxAttribute(int halfSize)
        {
            Rectangle = new Rectangle(-halfSize, -halfSize, halfSize * 2, halfSize * 2);
        }

        public BoxAttribute(int width, int height, int pivotX, int pivotY)
        {
            Rectangle = new Rectangle(-pivotX, -pivotY, width, height);
        }

        public BoxAttribute(int halfWidth, int halfHeight)
        {
            Rectangle = new Rectangle(-halfWidth, -halfHeight, halfWidth * 2, halfHeight * 2);
        }
    }
}