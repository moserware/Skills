using System;
using Moserware.Numerics;

namespace Moserware.Skills.TrueSkill
{
    internal static class DrawMargin
    {
        public static double GetDrawMarginFromDrawProbability(double drawProbability, double beta)
        {
            // Derived from TrueSkill technical report (MSR-TR-2006-80), page 6

            // draw probability = 2 * CDF(margin/(sqrt(n1+n2)*beta)) -1

            // implies
            //
            // margin = inversecdf((draw probability + 1)/2) * sqrt(n1+n2) * beta
            // n1 and n2 are the number of players on each team
            double margin = GaussianDistribution.InverseCumulativeTo(.5*(drawProbability + 1), 0, 1)*Math.Sqrt(1 + 1)*
                            beta;
            return margin;
        }
    }
}