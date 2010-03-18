using Moserware.Skills;
using Moserware.Skills.Elo;
using NUnit.Framework;

namespace UnitTests.Elo
{
    [TestFixture]
    public class GaussianEloCalculatorTest
    {
        [Test]
        public void GaussianEloCalculatorTests()
        {
            const double defaultKFactor = 24.0;
            var calc = new GaussianEloCalculator();

            EloAssert.AssertChessRating(calc, 1200, 1200, PairwiseComparison.Win, 1212, 1188);
            EloAssert.AssertChessRating(calc, 1200, 1200, PairwiseComparison.Draw, 1200, 1200);
            EloAssert.AssertChessRating(calc, 1200, 1200, PairwiseComparison.Lose, 1188, 1212);

            // verified using TrueSkill paper equation
            EloAssert.AssertChessRating(calc, 1200, 1000, PairwiseComparison.Win, 1200 + ((1 - 0.76024993890652326884) * defaultKFactor), 1000 - (1 - 0.76024993890652326884) * defaultKFactor);
            EloAssert.AssertChessRating(calc, 1200, 1000, PairwiseComparison.Draw, 1200 - (0.76024993890652326884 - 0.5) * defaultKFactor, 1000 + (0.76024993890652326884 - 0.5) * defaultKFactor);
            EloAssert.AssertChessRating(calc, 1200, 1000, PairwiseComparison.Lose, 1200 - 0.76024993890652326884 * defaultKFactor, 1000 + 0.76024993890652326884 * defaultKFactor);
        }               
    }
}
