using System.Collections.Generic;
using Moserware.Skills.TrueSkill;

namespace Moserware.Skills
{
    /// <summary>
    /// Calculates a TrueSkill rating using <see cref="FactorGraphTrueSkillCalculator"/>.
    /// </summary>
    public static class TrueSkillCalculator
    {
        // Keep a singleton around
        private static readonly SkillCalculator _Calculator
            = new FactorGraphTrueSkillCalculator();

        /// <summary>
        /// Calculates new ratings based on the prior ratings and team ranks.
        /// </summary>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <param name="teamRanks">The ranks of the teams where 1 is first place. For a tie, repeat the number (e.g. 1, 2, 2)</param>
        /// <returns>All the players and their new ratings.</returns>
        public static IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                IEnumerable
                                                                                    <IDictionary<TPlayer, Rating>> teams,
                                                                                params int[] teamRanks)
        {
            // Just punt the work to the full implementation
            return _Calculator.CalculateNewRatings(gameInfo, teams, teamRanks);
        }

        /// <summary>
        /// Calculates the match quality as the likelihood of all teams drawing.
        /// </summary>
        /// <typeparam name="TPlayer">The underlying type of the player.</typeparam>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <returns>The match quality as a percentage (between 0.0 and 1.0).</returns>
        public static double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                            IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            // Just punt the work to the full implementation
            return _Calculator.CalculateMatchQuality(gameInfo, teams);
        }
    }
}