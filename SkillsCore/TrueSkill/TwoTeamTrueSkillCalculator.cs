using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Skills.Numerics;

namespace Moserware.Skills.TrueSkill
{
    /// <summary>
    /// Calculates new ratings for only two teams where each team has 1 or more players.
    /// </summary>
    /// <remarks>
    /// When you only have two teams, the math is still simple: no factor graphs are used yet.
    /// </remarks>
    public class TwoTeamTrueSkillCalculator : SkillCalculator
    {
        public TwoTeamTrueSkillCalculator()
            : base(SupportedOptions.None, Range<TeamsRange>.Exactly(2), Range<PlayersRange>.AtLeast(1))
        {
        }

        /// <inheritdoc/>
        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                  IEnumerable
                                                                                      <IDictionary<TPlayer, Rating>>
                                                                                      teams, params int[] teamRanks)
        {
            Guard.ArgumentNotNull(gameInfo, "gameInfo");
            ValidateTeamCountAndPlayersCountPerTeam(teams);

            RankSorter.Sort(ref teams, ref teamRanks);

            IDictionary<TPlayer, Rating> team1 = teams.First();
            IDictionary<TPlayer, Rating> team2 = teams.Last();

            bool wasDraw = (teamRanks[0] == teamRanks[1]);

            var results = new Dictionary<TPlayer, Rating>();

            UpdatePlayerRatings(gameInfo,
                                results,
                                team1,
                                team2,
                                wasDraw ? PairwiseComparison.Draw : PairwiseComparison.Win);

            UpdatePlayerRatings(gameInfo,
                                results,
                                team2,
                                team1,
                                wasDraw ? PairwiseComparison.Draw : PairwiseComparison.Lose);

            return results;
        }

        private static void UpdatePlayerRatings<TPlayer>(GameInfo gameInfo,
                                                         IDictionary<TPlayer, Rating> newPlayerRatings,
                                                         IDictionary<TPlayer, Rating> selfTeam,
                                                         IDictionary<TPlayer, Rating> otherTeam,
                                                         PairwiseComparison selfToOtherTeamComparison)
        {
            double drawMargin = DrawMargin.GetDrawMarginFromDrawProbability(gameInfo.DrawProbability, gameInfo.Beta);
            double betaSquared = Square(gameInfo.Beta);
            double tauSquared = Square(gameInfo.DynamicsFactor);

            int totalPlayers = selfTeam.Count() + otherTeam.Count();

            double selfMeanSum = selfTeam.Values.Sum(r => r.Mean);
            double otherTeamMeanSum = otherTeam.Values.Sum(r => r.Mean);

            double c = Math.Sqrt(selfTeam.Values.Sum(r => Square(r.StandardDeviation))
                                 +
                                 otherTeam.Values.Sum(r => Square(r.StandardDeviation))
                                 +
                                 totalPlayers*betaSquared);

            double winningMean = selfMeanSum;
            double losingMean = otherTeamMeanSum;

            switch (selfToOtherTeamComparison)
            {
                case PairwiseComparison.Win:
                case PairwiseComparison.Draw:
                    // NOP
                    break;
                case PairwiseComparison.Lose:
                    winningMean = otherTeamMeanSum;
                    losingMean = selfMeanSum;
                    break;
            }

            double meanDelta = winningMean - losingMean;

            double v;
            double w;
            double rankMultiplier;

            if (selfToOtherTeamComparison != PairwiseComparison.Draw)
            {
                // non-draw case
                v = TruncatedGaussianCorrectionFunctions.VExceedsMargin(meanDelta, drawMargin, c);
                w = TruncatedGaussianCorrectionFunctions.WExceedsMargin(meanDelta, drawMargin, c);
                rankMultiplier = (int) selfToOtherTeamComparison;
            }
            else
            {
                // assume draw
                v = TruncatedGaussianCorrectionFunctions.VWithinMargin(meanDelta, drawMargin, c);
                w = TruncatedGaussianCorrectionFunctions.WWithinMargin(meanDelta, drawMargin, c);
                rankMultiplier = 1;
            }

            foreach (var teamPlayerRatingPair in selfTeam)
            {
                Rating previousPlayerRating = teamPlayerRatingPair.Value;

                double meanMultiplier = (Square(previousPlayerRating.StandardDeviation) + tauSquared)/c;
                double stdDevMultiplier = (Square(previousPlayerRating.StandardDeviation) + tauSquared)/Square(c);

                double playerMeanDelta = (rankMultiplier*meanMultiplier*v);
                double newMean = previousPlayerRating.Mean + playerMeanDelta;

                double newStdDev =
                    Math.Sqrt((Square(previousPlayerRating.StandardDeviation) + tauSquared)*(1 - w*stdDevMultiplier));

                newPlayerRatings[teamPlayerRatingPair.Key] = new Rating(newMean, newStdDev);
            }
        }

        /// <inheritdoc/>
        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                              IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            Guard.ArgumentNotNull(gameInfo, "gameInfo");
            ValidateTeamCountAndPlayersCountPerTeam(teams);

            // We've verified that there's just two teams
            ICollection<Rating> team1 = teams.First().Values;
            int team1Count = team1.Count();

            ICollection<Rating> team2 = teams.Last().Values;
            int team2Count = team2.Count();

            int totalPlayers = team1Count + team2Count;

            double betaSquared = Square(gameInfo.Beta);

            double team1MeanSum = team1.Sum(r => r.Mean);
            double team1StdDevSquared = team1.Sum(r => Square(r.StandardDeviation));

            double team2MeanSum = team2.Sum(r => r.Mean);
            double team2SigmaSquared = team2.Sum(r => Square(r.StandardDeviation));

            // This comes from equation 4.1 in the TrueSkill paper on page 8            
            // The equation was broken up into the part under the square root sign and 
            // the exponential part to make the code easier to read.

            double sqrtPart
                = Math.Sqrt(
                    (totalPlayers*betaSquared)
                    /
                    (totalPlayers*betaSquared + team1StdDevSquared + team2SigmaSquared)
                    );

            double expPart
                = Math.Exp(
                    (-1*Square(team1MeanSum - team2MeanSum))
                    /
                    (2*(totalPlayers*betaSquared + team1StdDevSquared + team2SigmaSquared))
                    );

            return expPart*sqrtPart;
        }
    }
}