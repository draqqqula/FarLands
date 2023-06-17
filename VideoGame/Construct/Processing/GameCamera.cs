﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// олицетворяет камеру
    /// камера следует за целью
    /// </summary>
    public class GameCamera
    {
        public Vector2 Position { get; private set; }

        public Vector2 LeftTopCorner { get { return new Vector2(Position.X - Window.Width / 2, Position.Y - Window.Height / 2); } }
        public Vector2 RightTopCorner { get { return new Vector2(Position.X + Window.Width / 2, Position.Y - Window.Height / 2); } }
        public Vector2 LeftBottomCorner { get { return new Vector2(Position.X - Window.Width / 2, Position.Y + Window.Height / 2); } }
        public Vector2 RightBottomCorner { get { return new Vector2(Position.X + Window.Width / 2, Position.Y + Window.Height / 2); } }


        /// <summary>
        /// применяет паралакс к абсолютной позиции объекта
        /// паралакс представляет собой коэффициент изменения абсолютной позиции объекта относительно наблюдателя
        /// </summary>
        /// <param name="position"></param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <returns></returns>
        public Vector2 ApplyParalax(Vector2 position, float dx, float dy)
        {
            return position - new Vector2((int)(LeftTopCorner.X * dx), (int)(LeftTopCorner.Y * dy));
        }
        public GameObject TargetObject { get; private set; }
        /// <summary>
        /// внутренние границы, закреплены на цели
        /// положение камеры не может выйти за их пределы
        /// </summary>
        public Rectangle InnerBorders { get; private set; }
        /// <summary>
        /// внешние границы за которыми камера не может видеть
        /// </summary>
        public Rectangle? OuterBorders { get; private set; }
        /// <summary>
        /// область пространства, попадающая в поле зрения камеры
        /// </summary>
        public Rectangle Window
        {
            get
            {
                var gameWindow = Global.Variables.MainGame.Window.ClientBounds;
                return new Rectangle { 
                    Location = new Point((int)Position.X - gameWindow.Width/2, (int)Position.Y - gameWindow.Height/2), 
                    Height = gameWindow.Height, 
                    Width = gameWindow.Width 
                };
            }
        }
        private double InterpolationFactor
        {
            get
            {
                return 1 - Math.Pow(1-0.95, Global.Variables.DeltaTime.TotalSeconds/0.4);
            }
        }

        public GameCamera(Vector2 position, Rectangle borders)
        {
            Position = position;
            InnerBorders = borders;
        }
        /// <summary>
        /// устанавливает внешние границы пространства, в котором камера может видеть
        /// </summary>
        /// <param name="borders"></param>
        public void SetOuterBorders(Rectangle borders)
        {
            OuterBorders = borders;
        }

        /// <summary>
        /// true если объект в поле зрения камеры
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Sees(GameObject obj)
        {
            return Window.Intersects(obj.Layout);
        }

        private Vector2 Lerp(Vector2 a, Vector2 b, float k)
        {
            return b + (a - b) * k;
        }

        private Vector2 FitInBorders(Vector2 a, Vector2 b, Rectangle R)
        {
            return new Vector2(
                Math.Min(Math.Max(a.X, b.X - R.Width / 2), b.X + R.Width / 2),
                Math.Min(Math.Max(a.Y, b.Y - R.Height / 2), b.Y + R.Height / 2)
                );
        }

        /// <summary>
        /// заставляет камеру следовать за объектом
        /// </summary>
        /// <param name="targetObject"></param>
        public void LinkTo(GameObject targetObject)
        {
            this.TargetObject = targetObject;
        }

        /// <summary>
        /// обновляет позицию камеры, учитывая интерполяцию, внешние и внутренние границы
        /// </summary>
        public void Update()
        {
            if (TargetObject != null)
            {
                var rawPos = FitInBorders(
                    Lerp(TargetObject.Position, Position, (float)InterpolationFactor)
                    , TargetObject.Position, InnerBorders);
                if (OuterBorders.HasValue)
                {
                    var outer = OuterBorders.Value;
                    Position = FitInBorders(rawPos, outer.Center.ToVector2(), new Rectangle(new Point(0, 0), new Point(outer.Width - Window.Width, outer.Height - Window.Height)));
                }
                else
                    Position = rawPos;
            }
        }
    }
}
