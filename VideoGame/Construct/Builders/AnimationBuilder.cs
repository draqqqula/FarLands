using Animations;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Animations
{
    /// <summary>
    /// Воссоздаёт пакет анимаций из png-файла и текстового файла с описанием каждого кадра
    /// </summary>
    public static class AnimationBuilder
    {
        private static AnimationFrame BuildFrame(string line)
        {
            var numbers = Regex.Split(line, ",");
            var positions = numbers.Take(6).Select(int.Parse).ToArray();

            TimeSpan duration = TimeSpan.FromSeconds(double.Parse(numbers[6]));
            Rectangle borders = new Rectangle(positions[0], positions[1], positions[2], positions[3]);
            Vector2 anchor = new Vector2(positions[4], positions[5]);

            return new AnimationFrame(borders, anchor, duration);
        }

        private static Dictionary<string, string> BuildAnimationProperties(string[] properties)
        {
            return properties.ToDictionary(e => Regex.Split(e, "=")[0], e => Regex.Split(e, "=")[1]);
        }

        private static Animation BuildAnimation(Match match, Texture2D sheet)
        {
            var animationProperties = BuildAnimationProperties(match.Groups["Settings"].Captures.Select(v => v.Value).ToArray());

            return new Animation(match.Groups["Name"].Value, sheet,
                match.Groups["Frames"].Captures.Select(v => BuildFrame(v.Value)).ToArray(),
                animationProperties);
        }

        /// <summary>
        /// Строит анимации, используя "*name*.png" как общий спрайт и "*name*_properites.txt" как описание анимаций
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Dictionary<string, Animation> BuildFromFiles(string name)
        {
            var animations = new Dictionary<string, Animation>();
            var sheet = Global.Variables.MainContent.Load<Texture2D>(name);

            var path = Path.Combine(Environment.CurrentDirectory, string.Concat(name, "_properties.txt"));
            var rawProperties = string.Join(' ', File.ReadAllLines(path));
            var properties = Regex.Matches(rawProperties, "#(?'Name'[^ ]+) (?>(?'Settings'[^, ]+),?)+(?> (?'Frames'[^# ]+))*[^#]?");

            foreach (Match match in properties)
            {
                animations.Add(match.Groups["Name"].Value, BuildAnimation(match, sheet));
            }

            return animations;
        }
    }
}
