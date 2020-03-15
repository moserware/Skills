using System;

namespace Moserware.Skills.Elo
{
    public class GaussianKFactor : KFactor
    {
        // From paper
        const double StableDynamicsKFactor = 24.0;

        public GaussianKFactor()
            : base(StableDynamicsKFactor)
        {
        }

        public GaussianKFactor(GameInfo gameInfo, double latestGameWeightingFactor)
            : base(latestGameWeightingFactor * gameInfo.Beta * Math.Sqrt(Math.PI))
        {
        }
    }
}
