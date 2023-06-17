using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// семья объектов
    /// позволяет вручную сгруппировать ряд объектов
    /// </summary>
    public class Family
    {
        public string Name { get; set; }
        
        /// <summary>
        /// паттерны, объекты которых включены в семью
        /// </summary>
        public readonly List<IPattern> Patterns;

        /// <summary>
        /// члены семьи
        /// </summary>
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

        /// <summary>
        /// пополняет семью паттернами, объекты которых будут включены в семью
        /// </summary>
        /// <param name="patterns"></param>
        public void AddPatterns(params IPattern[] patterns)
        {
            Patterns.AddRange(patterns);
        }
    }
}
