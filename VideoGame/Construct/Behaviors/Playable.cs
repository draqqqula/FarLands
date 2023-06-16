using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class Playable : IBehavior
    {
        public string Name => "Playable";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        private Dummy Dummy;
        private TextObject HealthBar;
        private GameObject DashBar;
        private int DashCount;

        public void Act()
        {
            StringBuilder healthText = new StringBuilder();
            for (int i = 0; i < Dummy.MaxHealth; i++)
                healthText.Append(i < Dummy.Health ? 'a' : 'b');
            HealthBar.Text = healthText.ToString();
            if (DashCount > 0)
                DashBar.SetAnimation(string.Concat("Bar", DashCount), 0);
            else
                DashBar.SetAnimation("Default", 0);
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public Playable(Dummy dummy, TextObject healthBar, GameObject dashBar, bool enabled)
        {
            Dummy = dummy;
            HealthBar = healthBar;
            DashBar = dashBar;
            DashCount = 3;
            Enabled = enabled;
        }
    }
}
