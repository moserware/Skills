cmake_minimum_required(VERSION 2.6)
set(CMAKE_MODULE_PATH ${PROJECT_SOURCE_DIR}/cmake/modules)
enable_language(CSharp)

csharp_add_library(Moserware.Skills
    Skills/Elo/DuellingEloCalculator.cs
    Skills/Elo/EloRating.cs
    Skills/Elo/FideEloCalculator.cs
    Skills/Elo/FideKFactor.cs
    Skills/Elo/GaussianEloCalculator.cs
    Skills/Elo/GaussianKFactor.cs
    Skills/Elo/KFactor.cs
    Skills/Elo/TwoPlayerEloCalculator.cs
    Skills/FactorGraphs/Factor.cs
    Skills/FactorGraphs/FactorGraph.cs
    Skills/FactorGraphs/FactorGraphLayer.cs
    Skills/FactorGraphs/FactorList.cs
    Skills/FactorGraphs/Message.cs
    Skills/FactorGraphs/Schedule.cs
    Skills/FactorGraphs/Variable.cs
    Skills/FactorGraphs/VariableFactory.cs
    Skills/GameInfo.cs
    Skills/Guard.cs
    Skills/ISupportPartialPlay.cs
    Skills/ISupportPartialUpdate.cs
    Skills/Numerics/GaussianDistribution.cs
    Skills/Numerics/Matrix.cs
    Skills/Numerics/Range.cs
    Skills/PairwiseComparison.cs
    Skills/PartialPlay.cs
    Skills/Player.cs
    Skills/PlayersRange.cs
    Skills/Properties/AssemblyInfo.cs
    Skills/RankSorter.cs
    Skills/Rating.cs
    Skills/SkillCalculator.cs
    Skills/Team.cs
    Skills/TeamsRange.cs
    Skills/TrueSkill/DrawMargin.cs
    Skills/TrueSkill/FactorGraphTrueSkillCalculator.cs
    Skills/TrueSkill/TrueSkillFactorGraph.cs
    Skills/TrueSkill/TruncatedGaussianCorrectionFunctions.cs
    Skills/TrueSkill/TwoTeamTrueSkillCalculator.cs
    Skills/TrueSkill/TwoPlayerTrueSkillCalculator.cs
    Skills/TrueSkill/Factors/GaussianFactor.cs
    Skills/TrueSkill/Factors/GaussianGreaterThanFactor.cs
    Skills/TrueSkill/Factors/GaussianLikelihoodFactor.cs
    Skills/TrueSkill/Factors/GaussianPriorFactor.cs
    Skills/TrueSkill/Factors/GaussianWeightedSumFactor.cs
    Skills/TrueSkill/Factors/GaussianWithinFactor.cs
    Skills/TrueSkill/Layers/IteratedTeamDifferencesInnerLayer.cs
    Skills/TrueSkill/Layers/PlayerPerformancesToTeamPerformancesLayer.cs
    Skills/TrueSkill/Layers/PlayerPriorValuesToSkillsLayer.cs
    Skills/TrueSkill/Layers/PlayerSkillsToPerformancesLayer.cs
    Skills/TrueSkill/Layers/TeamDifferencesComparisonLayer.cs
    Skills/TrueSkill/Layers/TeamPerformancesToTeamPerformanceDifferencesLayer.cs
    Skills/TrueSkill/Layers/TrueSkillFactorGraphLayer.cs
    Skills/TrueSkillCalculator.cs
)

# find_program(NUNIT_CONSOLE nunit-console)

csharp_add_library(UnitTests
    UnitTests/Elo/GaussianEloCalculatorTest.cs
    UnitTests/Elo/DuellingEloTest.cs
    UnitTests/Elo/EloAssert.cs
    UnitTests/Elo/FideEloCalculatorTest.cs
    UnitTests/Numerics/MatrixTests.cs
    UnitTests/Numerics/GaussianDistributionTests.cs
    UnitTests/Properties/AssemblyInfo.cs
    UnitTests/RankSorterTest.cs
    UnitTests/TrueSkill/DrawMarginTest.cs
    UnitTests/TrueSkill/FactorGraphTrueSkillCalculatorTests.cs
    UnitTests/TrueSkill/TrueSkillCalculatorTests.cs
    UnitTests/TrueSkill/TwoPlayerTrueSkillCalculatorTest.cs
    UnitTests/TrueSkill/TwoTeamTrueSkillCalculatorTest.cs
    REFERENCES Moserware.Skills nunit.framework
)
add_dependencies(UnitTests Moserware.Skills)
