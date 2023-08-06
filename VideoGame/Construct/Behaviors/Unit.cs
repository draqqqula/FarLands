using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices;

namespace VideoGame
{
    /// <summary>
    /// Описывает поведение объекта, который может предпринимать действия в зависимости от обстоятельств
    /// </summary>
    public class Unit<T> : Behavior where T : Sprite
    {
        private TimerHandler timerHandler;

        private Dictionary<string, int> StepCooldowns;

        private readonly Dictionary<string, UnitMove<T>> Actions;

        private KeyValuePair<string, UnitMove<T>> CurrentAction;

        public Dummy Target { get; set; }

        #region TARGET
        public void SetTarget(Dummy target)
        {
            if (target != Target)
            {
                LoseTarget();
                Target = target;
                TakeAction(SearchForAction(target));
            }
        }

        public void LoseTarget()
        {
            timerHandler.CheckAndTurnOff("Action");
            timerHandler.CheckAndTurnOff("Cooldown");
            Target = null;
        }
        #endregion

        #region ACTIONS

        public bool HasTarget
        {
            get => Target is not null;
        }

        private void ReactWith(KeyValuePair<string, UnitMove<T>> action)
        {
            if (CurrentAction.Value is not null)
                CurrentAction.Value.OnForcedBreak(Parent as T, Target, action.Value);
            timerHandler.Silence("Action");
            timerHandler.Silence("Cooldown");
            TakeAction(action);
        }

        public void ReactWith(string actionName)
        {
            ReactWith(new KeyValuePair<string, UnitMove<T>>(actionName, Actions[actionName]));
        }
        public void ReactWith<Move>() where Move : UnitMove<T>
        {
            ReactWith(typeof(Move).Name);
        }

        public void TakeAction(KeyValuePair<string, UnitMove<T>> action)
        {
            Step();
            CurrentAction = action;

            if (action.Value.HasCooldown)
                timerHandler.SetTimer(action.Key, action.Value.Cooldown, false);

            if (action.Value.IsTurnDependent)
                StepCooldowns[action.Key] = action.Value.StepsToTurn;

            if (!action.Value.IsEndless)
            {
                timerHandler.SetTimer("Action", action.Value.Duration, (unit) => action.Value.OnEnd(Parent as T, Target), false);
                if (action.Value.HasFreeSpan)
                    timerHandler.SetTimer("Cooldown", action.Value.FreeSpan + action.Value.Duration, (parent) => TakeAction(SearchForAction(Target)), false);
            }

            action.Value.OnStart(Parent as T, Target);
        }

        public KeyValuePair<string, UnitMove<T>> SearchForAction(Dummy target)
        {
            return Actions
                .Where(action => !ActionOnCooldown(action.Key))
                .DefaultIfEmpty()
                .OrderByDescending(action => action.Value.GetAttraction(Parent as T, target))
                .ThenBy(action => action.Value.Priority)
                .First();
        }

        public bool ActionOnCooldown(string name)
        {
            return timerHandler.CheckAndTurnOff(name) == TimerState.Running || StepCooldowns.ContainsKey(name);
        }

        public void Step()
        {
            foreach (var cooldown in StepCooldowns.Keys)
                StepCooldowns[cooldown] -= 1;
            StepCooldowns = StepCooldowns.Where(cooldown => cooldown.Value > 0).ToDictionary(e => e.Key, e => e.Value);
        }

        public string ActionName
        {
            get => CurrentAction.Key;
        }

        public double Progress
        {
            get
            {
                double progress;
                if (timerHandler.TryGetProgress("Action", out progress))
                    return progress;
                else
                    return 0;
            }
        }

        public void AddAction(string name, UnitMove<T> action)
        {
            Actions.Add(name, action);
        }

        public void AddActions(params (string name, UnitMove<T> action)[] actions)
        {
            foreach(var action in actions)
            {
                AddAction(action.name, action.action);
            }
        }
        #endregion

        #region IBEHAVIOR
        public override void Act(TimeSpan deltaTime)
        {
            if (CurrentAction.Value != null && !CurrentAction.Value.Continue(Parent as T, Target))
            {
                var action = CurrentAction.Value;
                timerHandler.CheckAndTurnOff("Action");

                action.OnEnd(Parent as T, Target);

                if (action.HasFreeSpan)
                {
                    timerHandler.Hold("Cooldown", action.FreeSpan, (unit) => TakeAction(SearchForAction(Target)), false);
                }
                else
                {
                    TakeAction(SearchForAction(Target));
                }

                CurrentAction = default;
            }
        }
        #endregion

        #region CONSTRUCTOR
        public Unit(TimerHandler timerHandler, bool enabled, params UnitMove<T>[] actions)
        {
            Actions = actions.ToDictionary(it => it.GetType().Name, it => it);
            StepCooldowns = new Dictionary<string, int>();
            this.timerHandler = timerHandler;
            Enabled = enabled;
        }
        #endregion
    }

    public abstract class UnitMove<T> where T : Sprite
    {
        #region STATUS
        public bool Enabled { get; private set; }
        public void Enable() => Enabled = true;
        public void Disable() => Enabled = false;
        #endregion

        #region PROPERTIES
        public readonly int Priority;
        public readonly TimeSpan Duration;
        public readonly TimeSpan Cooldown;
        public readonly int StepsToTurn;
        public readonly TimeSpan FreeSpan;
        #endregion

        #region RULES
        public readonly bool IsEndless;
        public readonly bool HasCooldown;
        public readonly bool IsTurnDependent;
        public readonly bool HasFreeSpan;

        #endregion

        #region SCRIPT
        public virtual void OnStart(T unit, Dummy target) { }
        public virtual void OnEnd(T unit, Dummy target) { }
        public virtual bool Continue(T unit, Dummy target) { return true; }
        public virtual void OnForcedBreak(T unit, Dummy target, UnitMove<T> breaker) { }
        #endregion

        #region CHOICE
        public abstract int GetAttraction(T unit, Dummy target);
        #endregion

        #region CREATION
        public UnitMove()
        {
            var attributes = GetType().GetCustomAttributes(true);
            Priority = 1;
            foreach (var attribute in attributes)
            {
                if (attribute is MoveDurationAttribute moveDuration)
                {
                    Duration = moveDuration.Duration;
                    IsEndless = false;
                }
                else if (attribute is EndlessMoveAttribute)
                {
                    Duration = TimeSpan.Zero;
                    IsEndless = true;
                }
                else if (attribute is MoveFreeSpanAttribute moveFreeSpan)
                {
                    FreeSpan = moveFreeSpan.Duration;
                    HasFreeSpan = true;
                }
                else if (attribute is NoFreeSpanMoveAttribute)
                {
                    FreeSpan = TimeSpan.Zero;
                    HasFreeSpan = false;
                }
                else if (attribute is MoveStepsRequiredAttribute moveStepsRequired)
                {
                    StepsToTurn = moveStepsRequired.Count;
                    IsTurnDependent = true;
                }
                else if (attribute is TurnIndependentMoveAttribute)
                {
                    StepsToTurn = 0;
                    IsTurnDependent = false;
                }
                else if (attribute is MoveCooldownAttribute moveCooldown)
                {
                    Cooldown = moveCooldown.Duration;
                    HasCooldown = true;
                }
                else if (attribute is NoCooldownMoveAttribute)
                {
                    Cooldown = TimeSpan.Zero;
                    HasCooldown = false;
                }
                else if (attribute is MovePriorityAttribute priority)
                {
                    Priority = priority.Number;
                }
            }
        }
        #endregion
    }
    #region UNITMOVE ATTRIBUTES
    public class MoveDurationAttribute : Attribute
    {
        public TimeSpan Duration { get; }
        public MoveDurationAttribute(double seconds)
        {
            Duration = TimeSpan.FromSeconds(seconds);
        }
    }
    public class MoveFreeSpanAttribute : Attribute
    {
        public TimeSpan Duration { get; }
        public MoveFreeSpanAttribute(double seconds)
        {
            Duration = TimeSpan.FromSeconds(seconds);
        }
    }

    public class MoveStepsRequiredAttribute : Attribute
    {
        public int Count { get; }
        public MoveStepsRequiredAttribute(int count)
        {
            Count = count;
        }
    }

    public class MovePriorityAttribute : Attribute
    {
        public int Number { get; }
        public MovePriorityAttribute(int number)
        {
            Number = number;
        }
    }

    public class MoveCooldownAttribute : Attribute
    {
        public TimeSpan Duration { get; }
        public MoveCooldownAttribute(double seconds)
        {
            Duration = TimeSpan.FromSeconds(seconds);
        }
    }

    public class EndlessMoveAttribute : Attribute { }
    public class TurnIndependentMoveAttribute : Attribute { }
    public class NoFreeSpanMoveAttribute : Attribute { }
    public class NoCooldownMoveAttribute : Attribute { }
    #endregion
}
