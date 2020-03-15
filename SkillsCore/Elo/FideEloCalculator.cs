using System;

namespace Moserware.Skills.Elo
{
    // Including ELO's scheme as a simple comparison. 
    // See http://en.wikipedia.org/wiki/Elo_rating_system#Theory
    // for more details
    public class FideEloCalculator : TwoPlayerEloCalculator
    {
        public FideEloCalculator()
            : this(new FideKFactor())
        {
        }

        public FideEloCalculator(FideKFactor kFactor)
            : base(kFactor)
        {
        }

        protected override double GetPlayerWinProbability(GameInfo gameInfo, double playerRating, double opponentRating)
        {
            double ratingDifference = opponentRating - playerRating;

            return 1.0
                   /
                   (
                       1.0 + Math.Pow(10.0, ratingDifference / (2 * gameInfo.Beta))
                   );
        }        
    }
}