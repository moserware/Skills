using Moserware.Skills;
using System;
using System.Diagnostics;

namespace $rootnamespace$ {
    public static class TrueSkillSamples {
        public static void RunSamples() {
            // Let's run a few examples. Simply call this method from your project.
            // Feel free to follow along in the debugger.
            // It's important to note that this sample file shows several different scenarios.
            // Pick the one that best matches your needs and delete the others

            TwoPlayerTestNotDrawn();
            ThreeOnTwoTests();
            TwoOnFourOnTwoWinDraw();
            OneOnTwoBalancedPartialPlay();
        }

        private static void TwoPlayerTestNotDrawn() {
            // Here's the most simple case: you have two players and one wins 
            // against the other.

            // Let's new up two players. Note that the argument passed into to Player
            // can be anything. This allows you to wrap any object. Here I'm just 
            // using a simple integer to represent the player, but you could just as
            // easily pass in a database entity representing a person/user or any
            // other custom class you have.

            var player1 = new Player(1);
            var player2 = new Player(2);

            // The algorithm has several parameters that can be tweaked that are
            // found in the "GameInfo" class. If you're just starting out, simply
            // use the defaults:
            var gameInfo = GameInfo.DefaultGameInfo;

            // A "Team" is a collection of "Player" objects. Here we have a team
            // that consists of single players.

            // Note that for each player on the team, we indicate that they have
            // the "DefaultRating" which means that the algorithm has never seen
            // them before. In a real implementation, you'd pull this previous
            // rating for the player based on the player.Id value. It could come
            // from a database.
            var team1 = new Team(player1, gameInfo.DefaultRating);
            var team2 = new Team(player2, gameInfo.DefaultRating);

            // We bundle up all of our teams together so that we can feed them to
            // the algorithm.
            var teams = Teams.Concat(team1, team2);

            // Before we know the actual results of the game, we can ask the 
            // calculator for what it perceives as the quality of the match (higher
            // means more fair/equitable)
            AssertMatchQuality(0.447, TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams));

            // This is the key line. We ask the calculator to calculate new ratings
            // Pay careful attention to the numbers at the end. This indicates that
            // team1 came in first place and team2 came in second place. TrueSkill
            // is flexible and allows scenarios such as team1 and team2 drawing which
            // could be represented as "1,1" since they both came in first place.
            var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            // The result of the calculation is a dictionary mapping the players to
            // their new rating. Here we get the ratings out for each player
            var player1NewRating = newRatings[player1];
            var player2NewRating = newRatings[player2];
            
            // In a real implementation, you'd store these values in a persistent
            // store like a database (note that you can use the player.Id to map
            // the Player class to the class of your choice.
            AssertRating(29.39583201999924, 7.171475587326186, player1NewRating);
            AssertRating(20.60416798000076, 7.171475587326186, player2NewRating);
        }

        private static void ThreeOnTwoTests() {
            // To make things interesting, here is a team of three people playing
            // a team of two people.

            // Initialize the players on the first team. Remember that the argument
            // passed to the Player constructor can be anything. It's strictly there
            // to help you uniquely identify people.
            var player1 = new Player(1);
            var player2 = new Player(2);
            var player3 = new Player(3);

            // Note the fluent-like API where you can add players to the Team and 
            // specify the rating of each using their mean and standard deviation
            // (for more information on these parameters, see the accompanying post
            // http://www.moserware.com/2010/03/computing-your-skill.html )
            var team1 = new Team()
                        .AddPlayer(player1, new Rating(28, 7))
                        .AddPlayer(player2, new Rating(27, 6))
                        .AddPlayer(player3, new Rating(26, 5));


            // Create players for the second team
            var player4 = new Player(4);
            var player5 = new Player(5);

            var team2 = new Team()
                        .AddPlayer(player4, new Rating(30, 4))
                        .AddPlayer(player5, new Rating(31, 3));

            // The default parameters are fine
            var gameInfo = GameInfo.DefaultGameInfo;

            // We only have two teams, combine the teams into one parameter
            var teams = Teams.Concat(team1, team2);

            // Specify that the outcome was a 1st and 2nd place
            var newRatingsWinLoseExpected = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);

            // Winners
            AssertRating(28.658, 6.770, newRatingsWinLoseExpected[player1]);
            AssertRating(27.484, 5.856, newRatingsWinLoseExpected[player2]);
            AssertRating(26.336, 4.917, newRatingsWinLoseExpected[player3]);

            // Losers
            AssertRating(29.785, 3.958, newRatingsWinLoseExpected[player4]);
            AssertRating(30.879, 2.983, newRatingsWinLoseExpected[player5]);

            // For fun, let's see what would have happened if there was an "upset" and the better players lost
            var newRatingsWinLoseUpset = TrueSkillCalculator.CalculateNewRatings(gameInfo, Teams.Concat(team1, team2), 2, 1);

            // Winners
            AssertRating(32.012, 3.877, newRatingsWinLoseUpset[player4]);
            AssertRating(32.132, 2.949, newRatingsWinLoseUpset[player5]);

            // Losers
            AssertRating(21.840, 6.314, newRatingsWinLoseUpset[player1]);
            AssertRating(22.474, 5.575, newRatingsWinLoseUpset[player2]);
            AssertRating(22.857, 4.757, newRatingsWinLoseUpset[player3]);

            // Note that we could have predicted this wasn't a very balanced game ahead of time because
            // it had low match quality.
            AssertMatchQuality(0.254, TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams));
        }

        private static void TwoOnFourOnTwoWinDraw() {
            // Let's really take advantage of the algorithm by having three teams play:

            // Default info is fine
            var gameInfo = GameInfo.DefaultGameInfo;

            // The first team:
            var player1 = new Player(1);
            var player2 = new Player(2);

            var team1 = new Team()
                .AddPlayer(player1, new Rating(40, 4))
                .AddPlayer(player2, new Rating(45, 3));

            // The second team:
            var player3 = new Player(3);
            var player4 = new Player(4);
            var player5 = new Player(5);
            var player6 = new Player(6);

            var team2 = new Team()
                        .AddPlayer(player3, new Rating(20, 7))
                        .AddPlayer(player4, new Rating(19, 6))
                        .AddPlayer(player5, new Rating(30, 9))
                        .AddPlayer(player6, new Rating(10, 4));

            var player7 = new Player(7);
            var player8 = new Player(8);

            // The third team:
            var team3 = new Team()
                        .AddPlayer(player7, new Rating(50, 5))
                        .AddPlayer(player8, new Rating(30, 2));


            // Put all three teams into one parameter:
            var teams = Teams.Concat(team1, team2, team3);

            // Note that we tell the calculator that there was a first place outcome of team 1, and then team 2 and 3 tied/drew
            var newRatingsWinLose = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2, 2);

            // Winners
            AssertRating(40.877, 3.840, newRatingsWinLose[player1]);
            AssertRating(45.493, 2.934, newRatingsWinLose[player2]);
            AssertRating(19.609, 6.396, newRatingsWinLose[player3]);
            AssertRating(18.712, 5.625, newRatingsWinLose[player4]);
            AssertRating(29.353, 7.673, newRatingsWinLose[player5]);
            AssertRating(9.872, 3.891, newRatingsWinLose[player6]);
            AssertRating(48.830, 4.590, newRatingsWinLose[player7]);
            AssertRating(29.813, 1.976, newRatingsWinLose[player8]);

            // We can even see match quality for the entire match consisting of three teams
            AssertMatchQuality(0.367, TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams));
        }

        private static void OneOnTwoBalancedPartialPlay() {
            // This scenario uses the "Partial Play" feature

            var gameInfo = GameInfo.DefaultGameInfo;

            // Player 1 is normal and just has a default rating
            // This player is the only person on the first team

            var p1 = new Player(1);
            var team1 = new Team(p1, gameInfo.DefaultRating);

            // Team 2 is much more interesting. Here we specify that
            // player 2 was on the team, but played for 0% of the game
            // and player 3 was on the team but played for 100% of the
            // game and thus should be updated appropriately.
            var p2 = new Player(2, 0.0);
            var p3 = new Player(3, 1.00);

            var team2 = new Team()
                        .AddPlayer(p2, gameInfo.DefaultRating)
                        .AddPlayer(p3, gameInfo.DefaultRating);

            var teams = Teams.Concat(team1, team2);
            var newRatings = TrueSkillCalculator.CalculateNewRatings(gameInfo, teams, 1, 2);
            var matchQuality = TrueSkillCalculator.CalculateMatchQuality(gameInfo, teams);
        }

        // The following helpers are just here to show the code is working properly. 
        // You don't need them in an actual program

        private static void AssertRating(double expectedMean, double expectedStandardDeviation, Rating actual) {
            const double ErrorTolerance = 0.085;
            Debug.Assert(Math.Abs(expectedMean - actual.Mean) < ErrorTolerance);
            Debug.Assert(Math.Abs(expectedStandardDeviation - actual.StandardDeviation) < ErrorTolerance);
        }

        private static void AssertMatchQuality(double expectedMatchQuality, double actualMatchQuality) {
            Debug.Assert(Math.Abs(expectedMatchQuality - actualMatchQuality) < 0.0005);
        }
    }
}
