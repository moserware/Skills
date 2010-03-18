using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers
{
    internal class TeamPerformancesToTeamPerformanceDifferencesLayer<TPlayer> :
        TrueSkillFactorGraphLayer
            <TPlayer, Variable<GaussianDistribution>, GaussianWeightedSumFactor, Variable<GaussianDistribution>>
    {
        public TeamPerformancesToTeamPerformanceDifferencesLayer(TrueSkillFactorGraph<TPlayer> parentGraph)
            : base(parentGraph)
        {
        }

        public override void BuildLayer()
        {
            for (int i = 0; i < InputVariablesGroups.Count - 1; i++)
            {
                Variable<GaussianDistribution> strongerTeam = InputVariablesGroups[i][0];
                Variable<GaussianDistribution> weakerTeam = InputVariablesGroups[i + 1][0];

                Variable<GaussianDistribution> currentDifference = CreateOutputVariable();
                AddLayerFactor(CreateTeamPerformanceToDifferenceFactor(strongerTeam, weakerTeam, currentDifference));

                // REVIEW: Does it make sense to have groups of one?
                OutputVariablesGroups.Add(new[] {currentDifference});
            }
        }

        private GaussianWeightedSumFactor CreateTeamPerformanceToDifferenceFactor(
            Variable<GaussianDistribution> strongerTeam, Variable<GaussianDistribution> weakerTeam,
            Variable<GaussianDistribution> output)
        {
            return new GaussianWeightedSumFactor(output, new[] {strongerTeam, weakerTeam}, new[] {1.0, -1.0});
        }

        private Variable<GaussianDistribution> CreateOutputVariable()
        {
            return ParentFactorGraph.VariableFactory.CreateBasicVariable("Team performance difference");
        }
    }
}