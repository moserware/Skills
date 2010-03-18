using Moserware.Skills.TrueSkill;
using NUnit.Framework;

namespace UnitTests.TrueSkill
{
    [TestFixture]
    public class FactorGraphTrueSkillCalculatorTests
    {
        [Test]
        public void FullFactorGraphCalculatorTests()
        {
            var calculator = new FactorGraphTrueSkillCalculator();

            // We can test all classes 
            TrueSkillCalculatorTests.TestAllTwoPlayerScenarios(calculator);
            TrueSkillCalculatorTests.TestAllTwoTeamScenarios(calculator);
            TrueSkillCalculatorTests.TestAllMultipleTeamScenarios(calculator);

            TrueSkillCalculatorTests.TestPartialPlayScenarios(calculator);
        }
    }
}