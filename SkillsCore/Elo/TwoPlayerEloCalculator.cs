using System;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.Elo
{    
    public abstract class TwoPlayerEloCalculator : SkillCalculator
    {
        protected readonly KFactor _KFactor;

        protected TwoPlayerEloCalculator(KFactor kFactor)
            : base(SupportedOptions.None, TeamsRange.Exactly(2), PlayersRange.Exactly(1))
        {
            _KFactor = kFactor;
        }

        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams);
            RankSorter.Sort(ref teams, ref teamRanks);

            var result = new Dictionary<TPlayer, Rating>();
            bool isDraw = (teamRanks[0] == teamRanks[1]);

            var player1 = teams.First().First();
            var player2 = teams.Last().First();
            
            var player1Rating = player1.Value.Mean;
            var player2Rating = player2.Value.Mean;

            result[player1.Key] = CalculateNewRating(gameInfo, player1Rating, player2Rating, isDraw ? PairwiseComparison.Draw : PairwiseComparison.Win);
            result[player2.Key] = CalculateNewRating(gameInfo, player2Rating, player1Rating, isDraw ? PairwiseComparison.Draw : PairwiseComparison.Lose);

            return result;
        }

        protected virtual EloRating CalculateNewRating(GameInfo gameInfo, double selfRating, double opponentRating, PairwiseComparison selfToOpponentComparison)
        {
            double expectedProbability = GetPlayerWinProbability(gameInfo, selfRating, opponentRating);
            double actualProbability = GetScoreFromComparison(selfToOpponentComparison);
            double k = _KFactor.GetValueForRating(selfRating);
            double ratingChange = k * (actualProbability - expectedProbability);
            double newRating = selfRating + ratingChange;

            return new EloRating(newRating);
        }

        private static double GetScoreFromComparison(PairwiseComparison comparison)
        {
            switch (comparison)
            {
                case PairwiseComparison.Win:
                    return 1;
                case PairwiseComparison.Draw:
                    return 0.5;
                case PairwiseComparison.Lose:
                    return 0;
                default:
                    throw new NotSupportedException();
            }
        }

        protected abstract double GetPlayerWinProbability(GameInfo gameInfo, double playerRating, double opponentRating);

        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            ValidateTeamCountAndPlayersCountPerTeam(teams);
            double player1Rating = teams.First().First().Value.Mean;
            double player2Rating = teams.Last().First().Value.Mean;
            double ratingDifference = player1Rating - player2Rating;

            // The TrueSkill paper mentions that they used s1 - s2 (rating difference) to
            // determine match quality. I convert that to a percentage as a delta from 50%
            // using the cumulative density function of the specific curve being used
            double deltaFrom50Percent = Math.Abs(GetPlayerWinProbability(gameInfo, player1Rating, player2Rating) - 0.5);
            return (0.5 - deltaFrom50Percent) / 0.5;
        }
    }
}
