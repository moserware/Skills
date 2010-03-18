using System;
using Moserware.Numerics;

namespace Moserware.Skills.Elo
{
    public class GaussianEloCalculator : TwoPlayerEloCalculator
    {
        // From the paper
        private static readonly KFactor StableKFactor = new KFactor(24);

        public GaussianEloCalculator()
            : base(StableKFactor)
        {
        }
        
        protected override double GetPlayerWinProbability(GameInfo gameInfo, double playerRating, double opponentRating)
        {
            double ratingDifference = playerRating - opponentRating;

            // See equation 1.1 in the TrueSkill paper
            return GaussianDistribution.CumulativeTo(
                ratingDifference
                /
                (Math.Sqrt(2) * gameInfo.Beta));
        }
    }
}
