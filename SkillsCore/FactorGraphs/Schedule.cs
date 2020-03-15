using System;
using System.Collections.Generic;

namespace Moserware.Skills.FactorGraphs
{
    public abstract class Schedule<T>
    {
        private readonly string _Name;

        protected Schedule(string name)
        {
            _Name = name;
        }

        public abstract double Visit(int depth, int maxDepth);

        public double Visit()
        {
            return Visit(-1, 0);
        }
                
        public override string ToString()
        {
            return _Name;
        }
    }

    public class ScheduleStep<T> : Schedule<T>
    {
        private readonly Factor<T> _Factor;
        private readonly int _Index;

        public ScheduleStep(string name, Factor<T> factor, int index)
            : base(name)
        {
            _Factor = factor;
            _Index = index;
        }

        public override double Visit(int depth, int maxDepth)
        {
            double delta = _Factor.UpdateMessage(_Index);
            return delta;
        }
    }
        
    public class ScheduleSequence<TValue> : ScheduleSequence<TValue, Schedule<TValue>>
    {
        public ScheduleSequence(string name, IEnumerable<Schedule<TValue>> schedules)
            : base(name, schedules)
        {
        }
    }

    public class ScheduleSequence<TValue, TSchedule> : Schedule<TValue>
        where TSchedule : Schedule<TValue>
    {
        private readonly IEnumerable<TSchedule> _Schedules;

        public ScheduleSequence(string name, IEnumerable<TSchedule> schedules)
            : base(name)
        {
            _Schedules = schedules;
        }

        public override double Visit(int depth, int maxDepth)
        {
            double maxDelta = 0;

            foreach (TSchedule currentSchedule in _Schedules)
            {
                maxDelta = Math.Max(currentSchedule.Visit(depth + 1, maxDepth), maxDelta);
            }
            
            return maxDelta;
        }
    }

    public class ScheduleLoop<T> : Schedule<T>
    {
        private readonly double _MaxDelta;
        private readonly Schedule<T> _ScheduleToLoop;

        public ScheduleLoop(string name, Schedule<T> scheduleToLoop, double maxDelta)
            : base(name)
        {
            _ScheduleToLoop = scheduleToLoop;
            _MaxDelta = maxDelta;
        }

        public override double Visit(int depth, int maxDepth)
        {
            int totalIterations = 1;
            double delta = _ScheduleToLoop.Visit(depth + 1, maxDepth);
            while (delta > _MaxDelta)
            {
                delta = _ScheduleToLoop.Visit(depth + 1, maxDepth);
                totalIterations++;
            }

            return delta;
        }
    }
}