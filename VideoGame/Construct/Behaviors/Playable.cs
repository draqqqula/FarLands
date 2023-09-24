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
    public class Playable : Behavior
    {
        const int MaxDashCount = 3;
        const double DashRecoverTime = 1;
        const double BlinkTime = 0.1;

        private Dummy Dummy;
        private float HealthDelta;
        private TimerHandler TimerHandler;
        private TextObject HealthBar;
        private Sprite DashBar;
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
                onDashRecovered(2);
            }
        }
        private event Action<int> onDashRecovered = delegate { };

        public void UseDash()
        {
            DashCount = Math.Max(DashCount - 1, 0);
            TimerHandler.SetTimer("RecoverDash", TimeSpan.FromSeconds(DashRecoverTime), (obj) => RecoverDash(), true);
        }

        private const double DecayTime = 0.1;
        private bool IsOnDecay = false;
        public override void Act(TimeSpan deltaTime)
        {
            StringBuilder healthText = new StringBuilder();
            for (int i = 1; i <= Dummy.MaxHealth; i++)
                healthText.Append(i <= Dummy.Health ? 'a' : (i <= HealthDelta ? 'c' : 'b'));
            HealthBar.Text = healthText.ToString();
            if (DashCount > 0)
                DashBar.SetAnimation(string.Concat("Bar", DashCount), 0);
            else
                DashBar.SetAnimation("Default", 0);

            if (HealthDelta > Dummy.Health)
            {
                if (!IsOnDecay && TimerHandler.When("HealthBarDecay", DecayTime))
                    IsOnDecay = true;
                else if (IsOnDecay)
                    HealthDelta = MathEx.Catch(MathEx.Lerp(HealthDelta, Dummy.Health, 0.2f, deltaTime), Dummy.Health, 0.04f);
            }
            else if (HealthDelta < Dummy.Health)
            {
                HealthDelta = Dummy.Health;
                IsOnDecay = false;
            }
            else
            {
                IsOnDecay = false;
            }
        }

        public Playable(Dummy dummy, TimerHandler timerHandler, TextObject healthBar, Sprite dashBar, bool enabled)
        {
            Dummy = dummy;
            TimerHandler = timerHandler;
            HealthBar = healthBar;
            DashBar = dashBar;
            DashCount = 3;
            Enabled = enabled;
            HealthDelta = dummy.Health;

            onDashRecovered += (n) =>
            {
                DashBar.IsVisible = false;
                TimerHandler.SetTimer("BlinkRecover", BlinkTime, (obj) => DashBar.IsVisible = true, true);
                TimerHandler.SetTimer("DashBar_Blink", TimeSpan.FromSeconds(BlinkTime * 2), (obj) => { if (n > 0) onDashRecovered(n - 1); }, false);
            };
        }
    }
}
