using System;
using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills.Elo
{
    public class DuellingEloCalculator : SkillCalculator
    {
        private readonly TwoPlayerEloCalculator _TwoPlayerEloCalc;

        public DuellingEloCalculator(TwoPlayerEloCalculator twoPlayerEloCalculator)
            : base(SupportedOptions.None, TeamsRange.AtLeast(2), PlayersRange.AtLeast(1))
        {
            _TwoPlayerEloCalc = twoPlayerEloCalculator;
        }

        public override IDictionary<TPlayer, Rating> CalculateNewRatings<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams, params int[] teamRanks)
        {
            // On page 6 of the TrueSkill paper, the authors write:
            // "When we had to process a team game or a game with more than two teams we used
            //  the so-called *duelling* heuristic: For each player, compute the Δ's in comparison
            //  to all other players based on the team outcome of the player and every other player and
            //  perform an update with the average of the Δ's."
            // This implements that algorithm.

            ValidateTeamCountAndPlayersCountPerTeam(teams);
            RankSorter.Sort(ref teams, ref teamRanks);

            var teamsList = teams.ToList();

            var deltas = new Dictionary<TPlayer, IDictionary<TPlayer, double>>();

            for(int ixCurrentTeam = 0; ixCurrentTeam < teamsList.Count; ixCurrentTeam++)
            {
                for(int ixOtherTeam = 0; ixOtherTeam < teamsList.Count; ixOtherTeam++)
                {
                    if(ixOtherTeam == ixCurrentTeam)
                    {
                        // Shouldn't duel against ourself ;)
                        continue;
                    }

                    var currentTeam = teamsList[ixCurrentTeam];
                    var otherTeam = teamsList[ixOtherTeam];

                    // Remember that bigger numbers mean worse rank (e.g. other-current is what we want)
                    var comparison = (PairwiseComparison) Math.Sign(teamRanks[ixOtherTeam] - teamRanks[ixCurrentTeam]);

                    foreach(var currentTeamPlayerRatingPair in currentTeam)
                    {
                        foreach(var otherTeamPlayerRatingPair in otherTeam)
                        {
                            UpdateDuels<TPlayer>(gameInfo, deltas, 
                                        currentTeamPlayerRatingPair.Key, currentTeamPlayerRatingPair.Value, 
                                        otherTeamPlayerRatingPair.Key, otherTeamPlayerRatingPair.Value, 
                                        comparison);

                        }
                    }
                }
            }
            
            var result = new Dictionary<TPlayer, Rating>();

            foreach(var currentTeam in teamsList)
            {
                foreach(var currentTeamPlayerPair in currentTeam)
                {
                    var currentPlayerAverageDuellingDelta = deltas[currentTeamPlayerPair.Key].Values.Average();
                    result[currentTeamPlayerPair.Key] = new EloRating(currentTeamPlayerPair.Value.Mean + currentPlayerAverageDuellingDelta);
                }
            }

            return result;
        }
        
        private void UpdateDuels<TPlayer>(GameInfo gameInfo,
                                          IDictionary<TPlayer, IDictionary<TPlayer, double>> duels, 
                                          TPlayer player1, Rating player1Rating, 
                                          TPlayer player2, Rating player2Rating, 
                                          PairwiseComparison weakToStrongComparison)
        {
            
            var duelOutcomes = _TwoPlayerEloCalc.CalculateNewRatings(gameInfo, 
                                                                     Teams.Concat(
                                                                       new Team<TPlayer>(player1, player1Rating),
                                                                       new Team<TPlayer>(player2, player2Rating)),
                                                                        (weakToStrongComparison == PairwiseComparison.Win) ? new int[] { 1, 2 }
                                                                      : (weakToStrongComparison == PairwiseComparison.Lose) ? new int[] { 2, 1 }
                                                                      : new int[] { 1, 1});


            UpdateDuelInfo(duels, player1, player1Rating, duelOutcomes[player1], player2);
            UpdateDuelInfo(duels, player2, player2Rating, duelOutcomes[player2], player1);
        }

        private static void UpdateDuelInfo<TPlayer>(IDictionary<TPlayer, IDictionary<TPlayer, double>> duels, 
                                                    TPlayer self, Rating selfBeforeRating, Rating selfAfterRating,
                                                    TPlayer opponent )
        {
            IDictionary<TPlayer, double> selfToOpponentDuelDeltas;

            if(!duels.TryGetValue(self, out selfToOpponentDuelDeltas))
            {
                selfToOpponentDuelDeltas = new Dictionary<TPlayer, double>();
                duels[self] = selfToOpponentDuelDeltas;
            }

            selfToOpponentDuelDeltas[opponent] = selfAfterRating.Mean - selfBeforeRating.Mean;
        }

        public override double CalculateMatchQuality<TPlayer>(GameInfo gameInfo, IEnumerable<IDictionary<TPlayer, Rating>> teams)
        {
            // HACK! Need a better algorithm, this is just to have something there and it isn't good
            double minQuality = 1.0;

            var teamList = teams.ToList();

            for(int ixCurrentTeam = 0; ixCurrentTeam < teamList.Count; ixCurrentTeam++)
            {
                EloRating currentTeamAverageRating = new EloRating(teamList[ixCurrentTeam].Values.Average(r => r.Mean));
                var currentTeam = new Team(new Player(ixCurrentTeam), currentTeamAverageRating);

                for(int ixOtherTeam = ixCurrentTeam + 1; ixOtherTeam < teamList.Count; ixOtherTeam++)
                {
                    EloRating otherTeamAverageRating = new EloRating(teamList[ixOtherTeam].Values.Average(r => r.Mean));
                    var otherTeam = new Team(new Player(ixOtherTeam), otherTeamAverageRating);

                    minQuality = Math.Min(minQuality,
                                          _TwoPlayerEloCalc.CalculateMatchQuality(gameInfo,
                                                                                  Teams.Concat(currentTeam, otherTeam)));
                }
            }

            return minQuality;
        }
    }
}