﻿using Animations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoGame
{
    /// <summary>
    /// Действие, которое может совершить Unit
    /// </summary>
    public class UnitAction
    {
        public bool Enabled { get; set; }

        public readonly TimeSpan Duration;
        public readonly TimeSpan Cooldown;
        private IEnumerable<Func<Unit,Dummy,int>> AttractionFactors { get; set; }

        public readonly Action<GameObject> StartTrigger;
        public readonly Func<Unit, Dummy, bool> RunCondition;
        public readonly Action<GameObject> EndTrigger;


        public int GetAttractiveness(Unit unit, Dummy target)
        {
            return AttractionFactors.Sum(e => e(unit, target));
        }

        public readonly bool TimeRelated;
        public readonly TimeSpan TimeUntilTurn;

        public readonly bool StepRelated;
        public readonly int StepsUntilTurn;

        public UnitAction(TimeSpan duration, TimeSpan cooldown, IEnumerable<Func<Unit, Dummy, int>> attractionFactors,
            Action<GameObject> startTrigger, Func<Unit, Dummy, bool> runCondition, Action<GameObject> endTrigger,
            TimeSpan? timeUntilTurn, int? stepsUntilTurn, bool enabled)
        {
            Duration = duration;
            Cooldown = cooldown;
            Enabled = enabled;
            AttractionFactors = attractionFactors;
            StartTrigger = startTrigger;
            RunCondition = runCondition;
            EndTrigger = endTrigger;

            TimeRelated = timeUntilTurn != null;
            TimeUntilTurn = TimeRelated ? timeUntilTurn.Value : TimeSpan.Zero;

            StepRelated = stepsUntilTurn != null;
            StepsUntilTurn = StepRelated ? stepsUntilTurn.Value : 0;
        }

        public UnitAction(TimeSpan duration, TimeSpan cooldown, 
            Action<GameObject> startTrigger, 
            Func<Unit, Dummy, bool> runCondition,
            Action<GameObject> endTrigger,
            TimeSpan? timeUntilTurn, int? stepsUntilTurn, bool enabled,
            params Func<Unit, Dummy, int>[] attractionFactors)
            :
            this(duration, cooldown, attractionFactors, startTrigger, runCondition, endTrigger, timeUntilTurn, stepsUntilTurn, enabled)
        {
        }
    }

    /// <summary>
    /// Описывает поведение объекта, который может предпринимать действия в зависимости от обстоятельств
    /// </summary>
    public class Unit : IBehavior
    {
        private TimerHandler timerHandler;
        public string Name => "Unit";


        private Dictionary<string, int> StepCooldowns { get; set; }

        private readonly Dictionary<string, UnitAction> Actions;

        private KeyValuePair<string, UnitAction> CurrentAction;

        public Dummy Target { get; set; }

        /// <summary>
        /// Предпринимает действие.
        /// </summary>
        /// <param name="action"></param>
        public void TakeAction(KeyValuePair<string, UnitAction> action)
        {
            Step();
            if (action.Value.TimeRelated)
                timerHandler.SetTimer(action.Key, action.Value.TimeUntilTurn, false);
            if (action.Value.StepRelated)
                StepCooldowns[action.Key] = action.Value.StepsUntilTurn;
            CurrentAction = action;
            timerHandler.SetTimer("Action", action.Value.Duration, action.Value.EndTrigger, false);
            timerHandler.SetTimer("Cooldown", action.Value.Cooldown, (parent) => TakeAction(SearchForAction(Target)), false);
            action.Value.StartTrigger(Parent);
        }

        /// <summary>
        /// Ищет, какое действие будет лучше всего предпринять в данный момент.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public KeyValuePair<string, UnitAction> SearchForAction(Dummy target)
        {
            return Actions
                .Where(action => !ActionOnCooldown(action.Key))
                .DefaultIfEmpty()
                .MaxBy(action => action.Value.GetAttractiveness(this, target));
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

        /// <summary>
        /// назначает цель
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(Dummy target)
        {
            if (target != Target)
            {
                LoseTarget();
                Target = target;
                TakeAction(SearchForAction(target));
            }
        }

        /// <summary>
        /// открепляет цель
        /// </summary>
        public void LoseTarget()
        {
            timerHandler.CheckAndTurnOff("Action");
            timerHandler.CheckAndTurnOff("Cooldown");
            Target = null;
        }

        public GameObject Parent { get; set; }
        public bool Enabled { get; set; }

        public void AddAction(string name, UnitAction action)
        {
            Actions.Add(name, action);
        }

        public void AddActions(params (string name, UnitAction action)[] actions)
        {
            foreach(var action in actions)
            {
                AddAction(action.name, action.action);
            }
        }

        public void Act(TimeSpan deltaTime)
        {
            if (CurrentAction.Value != null && !CurrentAction.Value.RunCondition(this, Target))
            {
                var action = CurrentAction.Value;
                timerHandler.TurnOff("Action");
                timerHandler.ResetIfRunning("Cooldown", action.Cooldown - action.Duration);

                action.EndTrigger(Parent);
                CurrentAction = default;
            }
        }

        public DrawingParameters ChangeAppearance(DrawingParameters parameters)
        {
            return parameters;
        }

        public Unit(TimerHandler timerHandler, bool enabled)
        {
            Actions = new Dictionary<string, UnitAction>();
            StepCooldowns = new Dictionary<string, int>();
            this.timerHandler = timerHandler;
            Enabled = enabled;
        }
    }
}
