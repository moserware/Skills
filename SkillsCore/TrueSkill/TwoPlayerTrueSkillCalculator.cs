using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Skills.Numerics;

namespace Moserware.Skills.TrueSkill
{
    /// <summary>
    /// Calculates the new ratings for only two players.
    /// </summary>
    /// <remarks>
    /// When you only have two players, a lot of the math simplifies. The main purpose of this class
    /// is to show the bare minimum of what a TrueSkill implementation should have.
    /// </remarks>
    public class TwoPlayerTrueSkillCalculator : SkillCalculator
    {
        public TwoPlayerTrueSkillCalculator()
            : base(SupportedOptions.None, Range<TeamsRange>.Exactly(2), Range<PlayersRange>.Exactly(1))
        {
        }

        /// <inheritdoc/>
        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                  IEnumerable
                                                                                      <IDictionary<TPlayer, Rating>>
                                                                                      teams, params int[] teamRanks)
        {
            // Basic argument checking
            Guard.ArgumentNotNull(gameInfo, "gameInfo");
            ValidateTeamCountAndPlayersCountPerTeam(teams);

            // Make sure things are in order
            RankSorter.Sort(ref teams, ref teamRanks);

            // Get the teams as a list to make it easier to index
            List<IDictionary<TPlayer, Rating>> teamList = teams.ToList();

            // Since we verified that each team has one player, we know the player is the first one
            IDictionary<TPlayer, Rating> winningTeam = teamList[0];
            TPlayer winner = winningTeam.Keys.First();
            Rating winnerPreviousRating = winningTeam[winner];

            IDictionary<TPlayer, Rating> losingTeam = teamList[1];
            TPlayer loser = losingTeam.Keys.First();
            Rating loserPreviousRating = losingTeam[loser];

            bool wasDraw = (teamRanks[0] == teamRanks[1]);

            var results = new Dictionary<TPlayer, Rating>();
            results[winner] = CalculateNewRating(gameInfo, winnerPreviousRating, loserPreviousRating,
                                                 wasDraw ? PairwiseComparison.Draw : PairwiseComparison.Win);
            results[loser] = CalculateNewRating(gameInfo, loserPreviousRating, winnerPreviousRating,
                                                wasDraw ? PairwiseComparison.Draw : PairwiseComparison.Lose);

            // And we're done!
            return results;
        }

        private static Rating CalculateNewRating(GameInfo gameInfo, Rating selfRating, Rating opponentRating,
                                                 PairwiseComparison comparison)
        {
            double drawMargin = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);

            double c =
                Math.Sqrt(
                    Square(selfRating.StandardDeviation)
                    +
                    Square(opponentRating.StandardDeviation)
                    +
                    2*Square(gameInfo.Beta));

            double winningMean = selfRating.Mean;
            double losingMean = opponentRating.Mean;

            switch (comparison)
            {
                case PairwiseComparison.Win:
                case PairwiseComparison.Draw:
                    // NOP
                    break;
                case PairwiseComparison.Lose:
                    winningMean = opponentRating.Mean;
                    losingMean = selfRating.Mean;
                    break;
            }

            double meanDelta = winningMean - losingMean;

            double v;
            double w;
            double rankMultiplier;

            if (comparison != PairwiseComparison.Draw)
            {
                // non-draw case
                v = TruncatedGaussianCorrectionFunctions.VExceedsMargin(meanDelta, drawMargin, c);
                w = TruncatedGaussianCorrectionFunctions.WExceedsMargin(meanDelta, drawMargin, c);
                rankMultiplier = (int) comparison;
            }
            else
            {
                v = TruncatedGaussianCorrectionFunctions.VWithinMargin(meanDelta, drawMargin, c);
                w = TruncatedGaussianCorrectionFunctions.WWithinMargin(meanDelta, drawMargin, c);
                rankMultiplier = 1;
            }

            double meanMultiplier = (Square(selfRating.StandardDeviation) + Square(gameInfo.DynamicsFactor))/c;

            double varianceWithDynamics = Square(selfRating.StandardDeviation) + Square(gameInfo.DynamicsFactor);
            double stdDevMultiplier = varianceWithDynamics/Square(c);

            double newMean = selfRating.Mean + (rankMultiplier*meanMultiplier*v);
            double newStdDev = Math.Sqrt(varianceWithDynamics*(1 - w*stdDevMultiplier));

            return new Rating(newMean, newStdDev);
        }

        /// <inheritdoc/>
        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                              IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            Guard.ArgumentNotNull(gameInfo, "gameInfo");
            ValidateTeamCountAndPlayersCountPerTeam(teams);

            Rating player1Rating = teams.First().Values.First();
            Rating player2Rating = teams.Last().Values.First();

            // We just use equation 4.1 found on page 8 of the TrueSkill 2006 paper:
            double betaSquared = Square(gameInfo.Beta);
            double player1SigmaSquared = Square(player1Rating.StandardDeviation);
            double player2SigmaSquared = Square(player2Rating.StandardDeviation);

            // This is the square root part of the equation:
            double sqrtPart =
                Math.Sqrt(
                    (2*betaSquared)
                    /
                    (2*betaSquared + player1SigmaSquared + player2SigmaSquared));

            // This is the exponent part of the equation:
            double expPart =
                Math.Exp(
                    (-1*Square(player1Rating.Mean - player2Rating.Mean))
                    /
                    (2*(2*betaSquared + player1SigmaSquared + player2SigmaSquared)));

            return sqrtPart*expPart;
        }
    }
}