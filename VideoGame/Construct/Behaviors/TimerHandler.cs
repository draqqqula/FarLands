using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    public class Timer
    {
        public TimeSpan AlarmTime { get; set; }
        public Action<GameObject> Alarm { get; set; }
        public bool IsOut { get; set; }
        public bool DeleteOnSurpass { get; set; }
        public double CheckProgress(TimeSpan time)
        {
            return Math.Min(Math.Max(0, time / AlarmTime), 1);
        }

        public bool Passes(TimeSpan time)
        {
            return time >= AlarmTime;
        }

        public Timer(TimeSpan time, Action<GameObject> alarm, bool deleteOnSurpass)
        {
            AlarmTime = time;
            Alarm = alarm;
            IsOut = false;
            DeleteOnSurpass = deleteOnSurpass;
        }
    }

    public enum TimerState
    {
        Running,
        IsOut,
        NotExists
    }

    public class TimerHandler : IBehavior
    {
        private Dictionary<string, Timer> Timers;

        public TimeSpan t;
        public string Name => "TimerHandler";

        public void SetTimer(string name, TimeSpan duration, Action<GameObject> alarm, bool deleteOnSurpass)
        {
            Timers[name] = new Timer(t + duration, alarm, deleteOnSurpass);
        }
        public void SetTimer(string name, TimeSpan duration, bool deleteOnSurpass)
        {
            SetTimer(name, t + duration, null, deleteOnSurpass);
        }
        public bool TryGetProgress(string name, out double value)
        {
            if (Timers.ContainsKey(name))
            {
                value = Timers[name].CheckProgress(t);
                return true;
            }
            else
            {
                value = -1;
                return false;
            }
        }
        public TimerState CheckAndTurnOff(string name)
        {
            if (!Timers.ContainsKey(name))
                return TimerState.NotExists;
            else
            {
                if (Timers[name].IsOut)
                {
                    TurnOff(name);
                    return TimerState.IsOut;
                }
                return TimerState.Running;
            }
        }

        public void TurnOff(string name)
        {
            if (Timers.ContainsKey(name))
                Timers.Remove(name);
        }

        public TimerState CheckAndDelay(string name, TimeSpan delay)
        {
            if (!Timers.ContainsKey(name))
                return TimerState.NotExists;
            else
            {
                var timer = Timers[name];
                if (timer.IsOut)
                {
                    SetTimer(name, delay, timer.Alarm, false);
                    return TimerState.IsOut;
                }
                return TimerState.Running;
            }
        }

        public TimerState CheckLooping(string name, TimeSpan delay, Action<GameObject> alarm)
        {
            var timerState = CheckAndDelay(name, delay);
            if (timerState == TimerState.NotExists)
                SetTimer(name, delay, alarm, false);
            return timerState;
        }

        public bool OnLoop(string name, TimeSpan delay, Action<GameObject> alarm)
        {
            return CheckLooping(name, delay, alarm) != TimerState.Running;
        }

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void Act()
        {
            t += Global.Variables.DeltaTime;
            var surpassed = Timers.Values
                .Where(timer => !timer.IsOut && timer.Passes(t));
            foreach (var timer in surpassed)
            {
                timer.IsOut = true;
                if (timer.Alarm != null)
                    timer.Alarm(Parent);
            }
            Timers = Timers.Where(timer => !(timer.Value.IsOut && timer.Value.DeleteOnSurpass)).ToDictionary(e => e.Key, e => e.Value);
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public TimerHandler(bool enabled)
        {
            Timers = new Dictionary<string, Timer>();
            Enabled = enabled;
            t = TimeSpan.Zero;
        }
    }
}
