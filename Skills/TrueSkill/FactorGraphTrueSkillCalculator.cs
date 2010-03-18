using System;
using System.Collections.Generic;
using System.Linq;
using Moserware.Numerics;

namespace Moserware.Skills.TrueSkill
{
    /// <summary>
    /// Calculates TrueSkill using a full factor graph.
    /// </summary>
    internal class FactorGraphTrueSkillCalculator : SkillCalculator
    {
        public FactorGraphTrueSkillCalculator()
            : base(SupportedOptions.PartialPlay | SupportedOptions.PartialUpdate, TeamsRange.AtLeast(2), PlayersRange.AtLeast(1))
        {
        }

        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo,
                                                                                  IEnumerable<IDictionary<TPlayer, Rating>> teams, 
                                                                                  params int[] teamRanks)
        {
            Guard.ArgumentNotNull(gameInfo, "gameInfo");
            ValidateTeamCountAndPlayersCountPerTeam(teams);

            RankSorter.Sort(ref teams, ref teamRanks);

            var factorGraph = new TrueSkillFactorGraph<TPlayer>(gameInfo, teams, teamRanks);
            factorGraph.BuildGraph();
            factorGraph.RunSchedule();

            double probabilityOfOutcome = factorGraph.GetProbabilityOfRanking();

            return factorGraph.GetUpdatedRatings();
        }


        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo,
                                                              IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            // We need to create the A matrix which is the player team assigments.
            List<IDictionary<TPlayer, Rating>> teamAssignmentsList = teams.ToList();
            Matrix skillsMatrix = GetPlayerCovarianceMatrix(teamAssignmentsList);
            Vector meanVector = GetPlayerMeansVector(teamAssignmentsList);
            Matrix meanVectorTranspose = meanVector.Transpose;

            Matrix playerTeamAssignmentsMatrix = CreatePlayerTeamAssignmentMatrix(teamAssignmentsList, meanVector.Rows);
            Matrix playerTeamAssignmentsMatrixTranspose = playerTeamAssignmentsMatrix.Transpose;

            double betaSquared = Square(gameInfo.Beta);

            Matrix start = meanVectorTranspose * playerTeamAssignmentsMatrix;
            Matrix aTa = (betaSquared * playerTeamAssignmentsMatrixTranspose) * playerTeamAssignmentsMatrix;
            Matrix aTSA = playerTeamAssignmentsMatrixTranspose * skillsMatrix * playerTeamAssignmentsMatrix;
            Matrix middle = aTa + aTSA;

            Matrix middleInverse = middle.Inverse;

            Matrix end = playerTeamAssignmentsMatrixTranspose * meanVector;

            Matrix expPartMatrix = -0.5 * (start * middleInverse * end);
            double expPart = expPartMatrix.Determinant;

            double sqrtPartNumerator = aTa.Determinant;
            double sqrtPartDenominator = middle.Determinant;
            double sqrtPart = sqrtPartNumerator / sqrtPartDenominator;

            double result = Math.Exp(expPart) * Math.Sqrt(sqrtPart);

            return result;
        }

        private static Vector GetPlayerMeansVector<TPlayer>(
            IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList)
        {
            // A simple vector of all the player means.
            return new Vector(GetPlayerRatingValues(teamAssignmentsList, rating => rating.Mean));
        }

        private static Matrix GetPlayerCovarianceMatrix<TPlayer>(
            IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList)
        {
            // This is a square matrix whose diagonal values represent the variance (square of standard deviation) of all
            // players.
            return
                new DiagonalMatrix(GetPlayerRatingValues(teamAssignmentsList, rating => Square(rating.StandardDeviation)));
        }

        // Helper function that gets a list of values for all player ratings
        private static IList<double> GetPlayerRatingValues<TPlayer>(
            IEnumerable<IDictionary<TPlayer, Rating>> teamAssignmentsList, Func<Rating, double> playerRatingFunction)
        {
            var playerRatingValues = new List<double>();

            foreach (var currentTeam in teamAssignmentsList)
            {
                foreach (Rating currentRating in currentTeam.Values)
                {
                    playerRatingValues.Add(playerRatingFunction(currentRating));
                }
            }

            return playerRatingValues;
        }

        private static Matrix CreatePlayerTeamAssignmentMatrix<TPlayer>(
            IList<IDictionary<TPlayer, Rating>> teamAssignmentsList, int totalPlayers)
        {
            // The team assignment matrix is often referred to as the "A" matrix. It's a matrix whose rows represent the players
            // and the columns represent teams. At Matrix[row, column] represents that player[row] is on team[col]
            // Positive values represent an assignment and a negative value means that we subtract the value of the next 
            // team since we're dealing with pairs. This means that this matrix always has teams - 1 columns.
            // The only other tricky thing is that values represent the play percentage.

            // For example, consider a 3 team game where team1 is just player1, team 2 is player 2 and player 3, and 
            // team3 is just player 4. Furthermore, player 2 and player 3 on team 2 played 25% and 75% of the time 
            // (e.g. partial play), the A matrix would be:

            // A = this 4x2 matrix:
            // |  1.00  0.00 |
            // | -0.25  0.25 |
            // | -0.75  0.75 |
            // |  0.00 -1.00 |

            var playerAssignments = new List<IEnumerable<double>>();
            int totalPreviousPlayers = 0;

            for (int i = 0; i < teamAssignmentsList.Count - 1; i++)
            {
                IDictionary<TPlayer, Rating> currentTeam = teamAssignmentsList[i];

                // Need to add in 0's for all the previous players, since they're not
                // on this team
                var currentRowValues = new List<double>(new double[totalPreviousPlayers]);
                playerAssignments.Add(currentRowValues);

                foreach (var currentRating in currentTeam)
                {
                    currentRowValues.Add(PartialPlay.GetPartialPlayPercentage(currentRating.Key));
                    // indicates the player is on the team
                    totalPreviousPlayers++;
                }

                IDictionary<TPlayer, Rating> nextTeam = teamAssignmentsList[i + 1];
                foreach (var nextTeamPlayerPair in nextTeam)
                {
                    // Add a -1 * playing time to represent the difference
                    currentRowValues.Add(-1 * PartialPlay.GetPartialPlayPercentage(nextTeamPlayerPair.Key));
                }
            }

            var playerTeamAssignmentsMatrix = new Matrix(totalPlayers, teamAssignmentsList.Count - 1, playerAssignments);

            return playerTeamAssignmentsMatrix;
        }
    }
}