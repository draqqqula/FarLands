using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Isolation;

namespace VideoGame
{
    public class MenuState : GameState
    {

        public override void OnTick(TimeSpan deltaTime)
        {
        }

        public MenuState() : base(true, true, true, true, true, true) 
        {
        }
    }
}
