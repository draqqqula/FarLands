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

    /// <summary>
    /// Предоставляет функционал для работы с таймерами
    /// </summary>
    public class TimerHandler : IBehavior
    {
        private Dictionary<string, Timer> Timers { get; set; }

        public TimeSpan t;
        public string Name => "TimerHandler";
        private Dictionary<string, Timer> TimerBuffer { get; set; }
        private List<string> TurnOffBuffer { get; set; }

        private void AddTimers()
        {
            foreach (var timer in TimerBuffer)
            {
                Timers[timer.Key] = timer.Value;
            }
            TimerBuffer.Clear();
        }

        private void RemoveTimers()
        {
            foreach (string name in TurnOffBuffer)
            {
                Timers.Remove(name);
            }
            TurnOffBuffer.Clear();
        }

        public void SetTimer(string name, TimeSpan duration, Action<GameObject> alarm, bool deleteOnSurpass)
        {
            TimerBuffer[name] = new Timer(t + duration, alarm, deleteOnSurpass);
        }
        public void SetTimer(string name, TimeSpan duration, bool deleteOnSurpass)
        {
            SetTimer(name, duration, null, deleteOnSurpass);
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
            var state = Check(name);
            if (state == TimerState.IsOut)
                TurnOff(name);
            return state;
        }

        public TimerState Check(string name)
        {
            if (!Timers.ContainsKey(name))
                return TimerState.NotExists;
            else
            {
                if (Timers[name].IsOut)
                {
                    return TimerState.IsOut;
                }
                return TimerState.Running;
            }
        }

        public Action<GameObject> TurnOff(string name)
        {
            if (Timers.ContainsKey(name))
                TurnOffBuffer.Add(name);
            return Timers[name].Alarm;
        }

        public TimerState CheckAndDelay(string name, TimeSpan delay)
        {
            var state = Check(name);
            if (state == TimerState.IsOut)
                SetTimer(name, delay, Timers[name].Alarm, false);
            return state;
        }

        public void ResetIfRunning(string name, TimeSpan duration)
        {
            if (Check(name) == TimerState.Running)
                Timers[name].AlarmTime = t + duration;
        }

        public void Hold(string name, TimeSpan duration, Action<GameObject> alarm, bool deleteOnSurpass)
        {
            if (Check(name) == TimerState.Running)
                Timers[name].AlarmTime = t + duration;
            else
                SetTimer(name, duration, alarm, deleteOnSurpass);
        }

        public void Hold(string name, TimeSpan duration, bool deleteOnSurpass)
        {
            if (Check(name) == TimerState.Running)
                Timers[name].AlarmTime = t + duration;
            else
                SetTimer(name, duration, deleteOnSurpass);
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

        public void Act(TimeSpan deltaTime)
        {
            RemoveTimers();
            AddTimers();
            t += deltaTime;
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
            TimerBuffer = new Dictionary<string, Timer>();
            TurnOffBuffer = new List<string>();
        }
    }
}
