using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers
{
    // The whole purpose of this is to do a loop on the bottom
    internal class IteratedTeamDifferencesInnerLayer<TPlayer> :
        TrueSkillFactorGraphLayer
            <TPlayer, Variable<GaussianDistribution>, GaussianWeightedSumFactor, Variable<GaussianDistribution>>
    {
        private readonly TeamDifferencesComparisonLayer<TPlayer> _TeamDifferencesComparisonLayer;

        private readonly TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer>
            _TeamPerformancesToTeamPerformanceDifferencesLayer;

        public IteratedTeamDifferencesInnerLayer(TrueSkillFactorGraph<TPlayer> parentGraph,
                                                 TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer>
                                                     teamPerformancesToPerformanceDifferences,
                                                 TeamDifferencesComparisonLayer<TPlayer> teamDifferencesComparisonLayer)
            : base(parentGraph)
        {
            _TeamPerformancesToTeamPerformanceDifferencesLayer = teamPerformancesToPerformanceDifferences;
            _TeamDifferencesComparisonLayer = teamDifferencesComparisonLayer;
        }

        public override IEnumerable<Factor<GaussianDistribution>> UntypedFactors
        {
            get
            {
                return
                    _TeamPerformancesToTeamPerformanceDifferencesLayer.UntypedFactors.Concat(
                        _TeamDifferencesComparisonLayer.UntypedFactors);
            }
        }

        public override void BuildLayer()
        {
            _TeamPerformancesToTeamPerformanceDifferencesLayer.SetRawInputVariablesGroups(InputVariablesGroups);
            _TeamPerformancesToTeamPerformanceDifferencesLayer.BuildLayer();

            _TeamDifferencesComparisonLayer.SetRawInputVariablesGroups(
                _TeamPerformancesToTeamPerformanceDifferencesLayer.GetRawOutputVariablesGroups());
            _TeamDifferencesComparisonLayer.BuildLayer();
        }

        public override Schedule<GaussianDistribution> CreatePriorSchedule()
        {
            Schedule<GaussianDistribution> loop = null;

            switch (InputVariablesGroups.Count)
            {
                case 0:
                case 1:
                    throw new InvalidOperationException();
                case 2:
                    loop = CreateTwoTeamInnerPriorLoopSchedule();
                    break;
                default:
                    loop = CreateMultipleTeamInnerPriorLoopSchedule();
                    break;
            }

            // When dealing with differences, there are always (n-1) differences, so add in the 1
            int totalTeamDifferences = _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors.Count;
            int totalTeams = totalTeamDifferences + 1;

            var innerSchedule = new ScheduleSequence<GaussianDistribution>(
                "inner schedule",
                new[]
                    {
                        loop,
                        new ScheduleStep<GaussianDistribution>(
                            "teamPerformanceToPerformanceDifferenceFactors[0] @ 1",
                            _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[0], 1),
                        new ScheduleStep<GaussianDistribution>(
                            String.Format("teamPerformanceToPerformanceDifferenceFactors[teamTeamDifferences = {0} - 1] @ 2",
                                          totalTeamDifferences),
                            _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[totalTeamDifferences - 1], 2)
                    }
                );

            return innerSchedule;
        }

        private Schedule<GaussianDistribution> CreateTwoTeamInnerPriorLoopSchedule()
        {
            return ScheduleSequence(
                new[]
                    {
                        new ScheduleStep<GaussianDistribution>(
                            "send team perf to perf differences",
                            _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[0],
                            0),
                        new ScheduleStep<GaussianDistribution>(
                            "send to greater than or within factor",
                            _TeamDifferencesComparisonLayer.LocalFactors[0],
                            0)
                    },
                "loop of just two teams inner sequence");
        }

        private Schedule<GaussianDistribution> CreateMultipleTeamInnerPriorLoopSchedule()
        {
            int totalTeamDifferences = _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors.Count;

            var forwardScheduleList = new List<Schedule<GaussianDistribution>>();

            for (int i = 0; i < totalTeamDifferences - 1; i++)
            {
                Schedule<GaussianDistribution> currentForwardSchedulePiece =
                    ScheduleSequence(
                        new Schedule<GaussianDistribution>[]
                            {
                                new ScheduleStep<GaussianDistribution>(
                                    String.Format("team perf to perf diff {0}",
                                                  i),
                                    _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[i], 0),
                                new ScheduleStep<GaussianDistribution>(
                                    String.Format("greater than or within result factor {0}",
                                                  i),
                                    _TeamDifferencesComparisonLayer.LocalFactors[i],
                                    0),
                                new ScheduleStep<GaussianDistribution>(
                                    String.Format("team perf to perf diff factors [{0}], 2",
                                                  i),
                                    _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[i], 2)
                            }, "current forward schedule piece {0}", i);

                forwardScheduleList.Add(currentForwardSchedulePiece);
            }

            var forwardSchedule =
                new ScheduleSequence<GaussianDistribution>(
                    "forward schedule",
                    forwardScheduleList);

            var backwardScheduleList = new List<Schedule<GaussianDistribution>>();

            for (int i = 0; i < totalTeamDifferences - 1; i++)
            {
                var currentBackwardSchedulePiece = new ScheduleSequence<GaussianDistribution>(
                    "current backward schedule piece",
                    new Schedule<GaussianDistribution>[]
                        {
                            new ScheduleStep<GaussianDistribution>(
                                String.Format("teamPerformanceToPerformanceDifferenceFactors[totalTeamDifferences - 1 - {0}] @ 0",
                                              i),
                                _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[
                                    totalTeamDifferences - 1 - i], 0),
                            new ScheduleStep<GaussianDistribution>(
                                String.Format("greaterThanOrWithinResultFactors[totalTeamDifferences - 1 - {0}] @ 0",
                                              i),
                                _TeamDifferencesComparisonLayer.LocalFactors[totalTeamDifferences - 1 - i], 0),
                            new ScheduleStep<GaussianDistribution>(
                                String.Format("teamPerformanceToPerformanceDifferenceFactors[totalTeamDifferences - 1 - {0}] @ 1",
                                              i),
                                _TeamPerformancesToTeamPerformanceDifferencesLayer.LocalFactors[
                                    totalTeamDifferences - 1 - i], 1)
                        }
                    );
                backwardScheduleList.Add(currentBackwardSchedulePiece);
            }

            var backwardSchedule =
                new ScheduleSequence<GaussianDistribution>(
                    "backward schedule",
                    backwardScheduleList);

            var forwardBackwardScheduleToLoop =
                new ScheduleSequence<GaussianDistribution>(
                    "forward Backward Schedule To Loop",
                    new Schedule<GaussianDistribution>[]
                        {
                            forwardSchedule, backwardSchedule
                        });

            const double initialMaxDelta = 0.0001;

            var loop = new ScheduleLoop<GaussianDistribution>(
                String.Format("loop with max delta of {0}",
                              initialMaxDelta),
                forwardBackwardScheduleToLoop,
                initialMaxDelta);

            return loop;
        }
    }
}