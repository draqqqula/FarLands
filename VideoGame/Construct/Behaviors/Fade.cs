using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// поведение объекта, который будет плавно исчезать
    /// </summary>
    public class Fade : IBehavior
    {
        public string Name => "Fade";

        public TimeSpan Delay { get; set; }
        public TimeSpan FadeDuration { get; set; }

        public TimeSpan t;
        public bool DestroyAfterFadeOut { get; set; }

        public float CurrentOpacity
        {
            get
            {
                return (float) (1 - Math.Min((Math.Max((t - Delay).TotalSeconds, 0) / FadeDuration.TotalSeconds), 1));
            }
        }

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void Act()
        {
            t += Global.Variables.DeltaTime;
            if (DestroyAfterFadeOut && CurrentOpacity == 0)
                Parent.Destroy();
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            parameters.Color *= CurrentOpacity;
            return parameters;
        }

        public Fade(TimeSpan delay, TimeSpan fadeDuration, TimeSpan t0, bool destroyAfterFadeOut, bool enabled)
        {
            Delay = delay;
            FadeDuration = fadeDuration;
            t = t0;
            Enabled = enabled;
            DestroyAfterFadeOut = destroyAfterFadeOut;
        }
    }
}
