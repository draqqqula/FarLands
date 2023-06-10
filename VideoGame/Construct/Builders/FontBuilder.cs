using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoGame
{
    public static class FontBuilder
    {
        public static (char, Rectangle, Rectangle) BuildCharacter(string line)
        {
            var parameters = Regex.Split(line, " ");
            char symbol = char.Parse(parameters[0]);
            int[] properties = parameters.Skip(1).Select(n => int.Parse(n)).ToArray();
            Rectangle border = new Rectangle(properties[0], properties[1], properties[2], properties[3]);
            Rectangle cropping = new Rectangle(properties[4], properties[5], properties[6], properties[7]);
            return (symbol, border, cropping);
        }

        public static (char, Rectangle, Rectangle)[] BuildFromFiles(string name)
        {
            var path = Path.Combine(Environment.CurrentDirectory, string.Concat(name, "_font.txt"));
            var lines = File.ReadAllLines(path);
            return lines.Select(BuildCharacter).OrderBy(e => e.Item1).ToArray();
        }
    }
}
