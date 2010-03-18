using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;
using Moserware.Skills.FactorGraphs;
using Moserware.Skills.TrueSkill.Factors;

namespace Moserware.Skills.TrueSkill.Layers
{
    // We intentionally have no Posterior schedule since the only purpose here is to 
    internal class PlayerPriorValuesToSkillsLayer<TPlayer> :
        TrueSkillFactorGraphLayer
            <TPlayer, DefaultVariable<GaussianDistribution>, GaussianPriorFactor,
            KeyedVariable<TPlayer, GaussianDistribution>>
    {
        private readonly IEnumerable<IDictionary<TPlayer, Rating>> _Teams;

        public PlayerPriorValuesToSkillsLayer(TrueSkillFactorGraph<TPlayer> parentGraph,
                                              IEnumerable<IDictionary<TPlayer, Rating>> teams)
            : base(parentGraph)
        {
            _Teams = teams;
        }

        public override void BuildLayer()
        {
            foreach (var currentTeam in _Teams)
            {
                var currentTeamSkills = new List<KeyedVariable<TPlayer, GaussianDistribution>>();

                foreach (var currentTeamPlayer in currentTeam)
                {
                    KeyedVariable<TPlayer, GaussianDistribution> playerSkill =
                        CreateSkillOutputVariable(currentTeamPlayer.Key);
                    AddLayerFactor(CreatePriorFactor(currentTeamPlayer.Key, currentTeamPlayer.Value, playerSkill));
                    currentTeamSkills.Add(playerSkill);
                }

                OutputVariablesGroups.Add(currentTeamSkills);
            }
        }

        public override Schedule<GaussianDistribution> CreatePriorSchedule()
        {
            return ScheduleSequence(
                from prior in LocalFactors
                select new ScheduleStep<GaussianDistribution>("Prior to Skill Step", prior, 0),
                "All priors");
        }

        private GaussianPriorFactor CreatePriorFactor(TPlayer player, Rating priorRating,
                                                      Variable<GaussianDistribution> skillsVariable)
        {
            return new GaussianPriorFactor(priorRating.Mean,
                                           Square(priorRating.StandardDeviation) +
                                           Square(ParentFactorGraph.GameInfo.DynamicsFactor), skillsVariable);
        }

        private KeyedVariable<TPlayer, GaussianDistribution> CreateSkillOutputVariable(TPlayer key)
        {
            return ParentFactorGraph.VariableFactory.CreateKeyedVariable(key, "{0}'s skill", key);
        }
    }
}