using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers
{
    internal class TeamDifferencesComparisonLayer<TPlayer> :
        TrueSkillFactorGraphLayer
            <TPlayer, Variable<GaussianDistribution>, GaussianFactor, DefaultVariable<GaussianDistribution>>
    {
        private readonly double _Epsilon;
        private readonly int[] _TeamRanks;

        public TeamDifferencesComparisonLayer(TrueSkillFactorGraph<TPlayer> parentGraph, int[] teamRanks)
            : base(parentGraph)
        {
            _TeamRanks = teamRanks;
            GameInfo gameInfo = ParentFactorGraph.GameInfo;
            _Epsilon = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);
        }

        public override void BuildLayer()
        {
            for (int i = 0; i < InputVariablesGroups.Count; i++)
            {
                bool isDraw = (_TeamRanks[i] == _TeamRanks[i + 1]);
                Variable<GaussianDistribution> teamDifference = InputVariablesGroups[i][0];

                GaussianFactor factor =
                    isDraw
                        ? (GaussianFactor) new GaussianWithinFactor(_Epsilon, teamDifference)
                        : new GaussianGreaterThanFactor(_Epsilon, teamDifference);

                AddLayerFactor(factor);
            }
        }
    }
}