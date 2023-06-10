using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class Family
    {
        public string Name { get; set; }

        public readonly List<IPattern> Patterns;

        public List<GameObject> Members
        {
            get
            {
                return Patterns.SelectMany(pattern => pattern.Editions).ToList();
            }
        }

        public Family(string name)
        {
            Name = name;
            Patterns = new List<IPattern>();
        }

        public void AddPatterns(params IPattern[] patterns)
        {
            Patterns.AddRange(patterns);
        }
    }
}
