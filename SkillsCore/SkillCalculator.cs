using System;
using System.Collections.Generic;

namespace Moserware.Skills
{    
    /// <summary>
    /// Base class for all skill calculator implementations.
    /// </summary>
    public abstract class SkillCalculator
    {
        [Flags]
        public enum SupportedOptions
        {
            None          = 0x00,
            PartialPlay   = 0x01,
            PartialUpdate = 0x02,
        }

        private readonly SupportedOptions _SupportedOptions;
        private readonly PlayersRange _PlayersPerTeamAllowed;
        private readonly TeamsRange _TotalTeamsAllowed;
        
        protected SkillCalculator(SupportedOptions supportedOptions, TeamsRange totalTeamsAllowed, PlayersRange playerPerTeamAllowed)
        {
            _SupportedOptions = supportedOptions;
            _TotalTeamsAllowed = totalTeamsAllowed;
            _PlayersPerTeamAllowed = playerPerTeamAllowed;
        }

        /// <summary>
        /// Calculates new ratings based on the prior ratings and team ranks.
        /// </summary>
        /// <typeparam name="TPlayer">The underlying type of the player.</typeparam>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <param name="teamRanks">The ranks of the teams where 1 is first place. For a tie, repeat the number (e.g. 1, 2, 2)</param>
        /// <returns>All the players and their new ratings.</returns>
        public abstract IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                  IEnumerable
                                                                                      <IDictionary<TPlayer, Rating>>
                                                                                      teams,
                                                                                  params int[] teamRanks);

        /// <summary>
        /// Calculates the match quality as the likelihood of all teams drawing.
        /// </summary>
        /// <typeparam name="TPlayer">The underlying type of the player.</typeparam>
        /// <param name="gameInfo">Parameters for the game.</param>
        /// <param name="teams">A mapping of team players and their ratings.</param>
        /// <returns>The quality of the match between the teams as a percentage (0% = bad, 100% = well matched).</returns>
        public abstract double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                              IEnumerable<IDictionary<TPlayer, Rating>> teams);

        public bool IsSupported(SupportedOptions option)
        {           
            return (_SupportedOptions & option) == option;             
        }

        /// <summary>
        /// Helper function to square the <paramref name="value"/>.
        /// </summary>        
        /// <returns><param name="value"/> * <param name="value"/></returns>
        protected static double Square(double value)
        {
            return value*value;
        }

        protected void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams, _TotalTeamsAllowed, _PlayersPerTeamAllowed);
        }

        private static void ValidateTeamCountAndPlayersCountPerTeam<TPlayer>(
            IEnumerable<IDictionary<TPlayer, Rating>> teams, TeamsRange totalTeams, PlayersRange playersPerTeam)
        {
            Guard.ArgumentNotNull(teams, "teams");
            int countOfTeams = 0;
            foreach (var currentTeam in teams)
            {
                if (!playersPerTeam.IsInRange(currentTeam.Count))
                {
                    throw new ArgumentException();
                }
                countOfTeams++;
            }

            if (!totalTeams.IsInRange(countOfTeams))
            {
                throw new ArgumentException();
            }
        }
    }
}