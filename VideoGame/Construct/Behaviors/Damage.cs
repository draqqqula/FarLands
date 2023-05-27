using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame.Construct.Behaviors
{
    public class Damage : Collider
    {
        public string Name => "Damage";

        public GameObject Parent { get; set; }

        public void Act()
        {
            throw new NotImplementedException();
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }
    }
}
