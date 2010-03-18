using Moserware.Skills.TrueSkill;
using NUnit.Framework;

namespace UnitTests.TrueSkill
{
    [TestFixture]
    public class TwoTeamTrueSkillCalculatorTest
    {
        [Test]
        public void TwoTeamTrueSkillCalculatorTests()
        {
            var calculator = new TwoTeamTrueSkillCalculator();

            // This calculator supports up to two teams with many players each
            TrueSkillCalculatorTests.TestAllTwoPlayerScenarios(calculator);
            TrueSkillCalculatorTests.TestAllTwoTeamScenarios(calculator);
        }
    }
}