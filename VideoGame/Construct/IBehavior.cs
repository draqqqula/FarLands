using Animations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public interface IBehavior
    {
        public string Name { get; }
        public GameObject Parent { get; set; }
        public DrawingParameters ChangeAppearance(DrawingParameters parameters);
        public void Act();
    }
}
