using Moserware.Skills;
using Moserware.Skills.Elo;
using NUnit.Framework;

namespace UnitTests.Elo
{
    [TestFixture]
    public class DuellingEloTest
    {
        private const double ErrorTolerance = 0.1;

        [Test]
        public void TwoOnTwoDuellingTest()
        {
            var calculator = new DuellingEloCalculator(new GaussianEloCalculator());

            var player1 = new Player(1);
            var player2 = new Player(2);

            var gameInfo = GameInfo.DefaultGameInfo;

            var team1 = new Team()
                .AddPlayer(player1, gameInfo.DefaultRating)
                .AddPlayer(player2, gameInfo.DefaultRating);

            var player3 = new Player(3);
            var player4 = new Player(4);

            var team2 = new Team()
                        .AddPlayer(player3, gameInfo.DefaultRating)
                        .AddPlayer(player4, gameInfo.DefaultRating);

            var teams = Teams.Concat(team1, team2);
            var newRatingsWinLose = calculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            // TODO: Verify?
            AssertRating(37, newRatingsWinLose[player1]);
            AssertRating(37, newRatingsWinLose[player2]);
            AssertRating(13, newRatingsWinLose[player3]);
            AssertRating(13, newRatingsWinLose[player4]);

            var quality = calculator.CalculateMatchQuality(gameInfo, teams);
            Assert.AreEqual(1.0, quality, 0.001);
        }

        private static void AssertRating(double expected, Rating actual)
        {
            Assert.AreEqual(expected, actual.Mean, ErrorTolerance);
        }
    }
}
