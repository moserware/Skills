using Moserware.Skills.TrueSkill;
using NUnit.Framework;

namespace UnitTests.TrueSkill
{
    [TestFixture]
    public class TwoPlayerTrueSkillCalculatorTest
    {
        [Test]
        public void TwoPlayerTrueSkillCalculatorTests()
        {
            var calculator = new TwoPlayerTrueSkillCalculator();

            // We only support two players
            TrueSkillCalculatorTests.TestAllTwoPlayerScenarios(calculator);

            // TODO: Assert failures for larger teams
        }    
    }
}