using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VideoGame;

namespace Animations
{
    /// <summary>
    /// параметры, применяемые при отрисовке кадра
    /// </summary>
    public struct DrawingParameters
    {
        public Vector2 Position;
        public Color Color;
        public float Rotation;
        public Vector2 Scale;
        public SpriteEffects Mirroring;
        public Layer Layer;
        public float Priority;

        public DrawingParameters()
        {
            Position = new Vector2(0, 0);
            Color = Color.White;
            Rotation = 0f;
            Scale = new Vector2(3, 3);
            Mirroring = SpriteEffects.None;
            Layer = new Layer("new", a => a, 0);
            Priority = 0;
        }
    }

    /// <summary>
    /// Предоставляет функционал для управления анимациями
    /// </summary>
    public class Animator
    {
        private readonly Dictionary<string, Animation> Animations;
        public TimeSpan RunDuration { get; private set; }
        /// <summary>
        /// текущая анимация
        /// </summary>
        public Animation Running { get; private set; }
        public bool OnPause { get; private set; }

        /// <summary>
        /// проигрывает анимацию далее
        /// по завершении анимации переключает на следующую если та задана
        /// если же следующая анимация не задана а у текущей Looping == true проигрывает текущую анимацию с начала
        /// в остальных случаях при завершении начинает проигрывание анимации Default
        /// </summary>
        /// <param name="arguments"></param>
        public void Update(DrawingParameters arguments, TimeSpan deltaTime)
        {
            if (!OnPause)
            {
                RunDuration += deltaTime;
            }
            if (!Running.Run(RunDuration, arguments, this) && !OnPause)
            {
                if (Running.NextAnimation != null)
                    ChangeAnimation(arguments, Running.NextAnimation, 0);

                else if (Running.Looping)
                    ChangeAnimation(arguments, Running.Name, 0);

                else
                    ChangeAnimation(arguments, "Default", 0);
            }
        }

        /// <summary>
        /// продолжает проигрывание текущей анимации с этого кадра
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="frame"></param>
        public void SetFrame(DrawingParameters arguments, int frame)
        {
            ChangeAnimation(arguments, Running.Name, frame);
        }

        public void Stop() => OnPause = true;
        public void Resume() => OnPause = false;
        public void TogglePause() => OnPause = !OnPause;

        /// <summary>
        /// переключает анимацию и начинает проигрывание с этого кадра
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="animation"></param>
        /// <param name="initialFrame"></param>
        public void ChangeAnimation(DrawingParameters arguments, string animation, int initialFrame)
        {
            Running = Animations[animation];
            RunDuration = Running.Duration * Running.SpeedFactor * (initialFrame / (double)Running.FrameCount);
            Resume();
            Update(arguments, TimeSpan.Zero);
        }

        public Animator(Dictionary<string, Animation> animations, string initial)
        {
            if (animations.ContainsKey("Default"))
            {
                OnPause = false;
                Animations = animations;
                Running = Animations[initial];
                RunDuration = TimeSpan.Zero;
            }
            else
            {
                throw new FormatException("\"Default\" animation not found");
            }
        }

        public Animator(string animationsFile, string initial) :
            this(AnimationBuilder.BuildFromFiles(animationsFile), initial)
        {
        }
    }

    /// <summary>
    /// содержит информацию о кадрах
    /// </summary>
    public class Animation
    {
        private readonly AnimationFrame[] Frames;
        private readonly Texture2D Sheet;

        public readonly string Name;
        /// <summary>
        /// если true и не задан NextAnimation то по окончании текущей анимации начинает её проигрывание с начала
        /// </summary>
        public readonly bool Looping;
        /// <summary>
        /// общая длительность анимации
        /// </summary>
        public readonly TimeSpan Duration;
        /// <summary>
        /// если задано то по окончании текущей анимации начнёт проигрывать эту с первого кадра
        /// </summary>
        public string NextAnimation;
        /// <summary>
        /// коефициент скорости проигрывания
        /// </summary>
        public double SpeedFactor;

        public readonly int FrameCount;
        public int CurrentFrame { get; private set; }

        public Animation(string name, Texture2D sheet, AnimationFrame[] frames, Dictionary<string, string> properties)
        {
            var property =
            (string key, string _default) =>
            { if (properties.ContainsKey(key)) return properties[key]; else return _default; };

            Looping = bool.Parse(property("Looping", "false"));
            NextAnimation = property("NextAnimation", null);
            SpeedFactor = double.Parse(property("SpeedFactor", "1"));

            Frames = frames;
            Name = name;
            FrameCount = frames.Length;
            Sheet = sheet;
            Duration = TimeSpan.FromSeconds(frames.Sum(frame => frame.Duration.TotalSeconds));

            CurrentFrame = 0;
        }

        /// <summary>
        /// если возможно, выбирает кадр отталкиваясь от прогресса анимации, отправляет в буфер на отрисовку и возвращает true
        /// если невозможно выбрать кадр, возвращает false
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="arguments"></param>
        /// <param name="animator"></param>
        /// <returns></returns>
        public bool Run(double progress, DrawingParameters arguments, Animator animator)
        {
            if (progress > 1 || progress < 0)
            {
                return false;
            }

            CurrentFrame = (int)Math.Round(progress * (Frames.Length - 1));
            Frames[CurrentFrame].CreateDrawable(arguments, Sheet, animator);
            return true;
        }

        /// <summary>
        /// если возможно, выбирает кадр отталкиваясь от длительности анимации, отправляет в буфер на отрисовку и возвращает true
        /// если невозможно выбрать кадр, возвращает false
        /// </summary>
        /// <param name="t"></param>
        /// <param name="arguments"></param>
        /// <param name="animator"></param>
        /// <returns></returns>
        public bool Run(TimeSpan t, DrawingParameters arguments, Animator animator)
        {
            return Run(t / SpeedFactor / Duration, arguments, animator);
        }
    }

    /// <summary>
    /// служит для сохранения кадра в буфере отрисовки
    /// </summary>
    public class DrawableElement
    {
        public AnimationFrame frame;
        public DrawingParameters arguments;
        public Texture2D sheet;

        public DrawableElement(AnimationFrame frame, DrawingParameters arguments, Texture2D sheet)
        {
            this.frame = frame;
            this.arguments = arguments;
            this.sheet = sheet;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            frame.Display(spriteBatch, arguments, sheet);
        }
    }

    /// <summary>
    /// кадр анимации
    /// задаётся областью на общем изображении, якорем и длительностью
    /// </summary>
    public class AnimationFrame
    {
        /// <summary>
        /// область на общем изображении
        /// </summary>
        private Rectangle Borders;
        /// <summary>
        /// точка которая будет совпадать с позицией объекта
        /// </summary>
        private Vector2 Anchor;
        /// <summary>
        /// длительность кадра
        /// </summary>
        public TimeSpan Duration;

        public AnimationFrame(Rectangle borders, Vector2 anchor, TimeSpan duration)
        {
            Borders = borders;
            Anchor = anchor;
            Duration = duration;
        }

        public AnimationFrame(int x, int y, int width, int height, int x0, int y0, double duration) :
            this(new Rectangle(x, y, width, height), new Vector2(x0, y0), TimeSpan.FromSeconds(duration))
        {
        }

        /// <summary>
        /// отправляет кадр в буфер отрисовки
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sheet"></param>
        /// <param name="animator"></param>
        public void CreateDrawable(DrawingParameters arguments, Texture2D sheet, Animator animator)
        {
            arguments.Layer.DrawBuffer[animator] = new DrawableElement(this, arguments, sheet);
        }

        /// <summary>
        /// выводит кадр на экран
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="sheet"></param>
        public void Display(SpriteBatch spriteBatch,DrawingParameters arguments, Texture2D sheet)
        {
            Vector2 offset = Vector2.Zero;
            if (arguments.Mirroring == SpriteEffects.FlipHorizontally)
                offset = new Vector2(Anchor.X * 2 - Borders.Width, 0) * arguments.Scale;
            spriteBatch.Draw(
                sheet,
                arguments.Position + offset,
                Borders,
                arguments.Color,
                arguments.Rotation,
                Anchor,
                arguments.Scale,
                arguments.Mirroring,
                arguments.Priority
                );
        }
    }
}
