using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Описывает поведение объекта, который может воздействовать на интерфейс
    /// </summary>
    public class Playable : IBehavior
    {
        const int MaxDashCount = 3;
        const double DashRecoverTime = 1;
        public string Name => "Playable";

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        private Dummy Dummy;
        private TimerHandler TimerHandler;
        private TextObject HealthBar;
        private GameObject DashBar;
        private int DashCount;

        public bool CanDash
        {
            get => DashCount > 0;
        }

        public void RecoverDash()
        {
            if (DashCount < MaxDashCount)
            {
                DashCount += 1;
                TimerHandler.SetTimer("RecoverDash", TimeSpan.FromSeconds(DashRecoverTime), (obj) => RecoverDash(), true);
            }
        }

        public void UseDash()
        {
            DashCount = Math.Max(DashCount - 1, 0);
            TimerHandler.SetTimer("RecoverDash", TimeSpan.FromSeconds(DashRecoverTime), (obj) => RecoverDash(), true);
        }

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

        public Playable(Dummy dummy, TimerHandler timerHandler, TextObject healthBar, GameObject dashBar, bool enabled)
        {
            Dummy = dummy;
            TimerHandler = timerHandler;
            HealthBar = healthBar;
            DashBar = dashBar;
            DashCount = 3;
            Enabled = enabled;
        }
    }
}
