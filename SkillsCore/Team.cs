using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills
{
    /// <summary>
    /// Helper class for working with a single team.
    /// </summary>
    public class Team<TPlayer>
    {
        private readonly Dictionary<TPlayer, Rating> _PlayerRatings = new Dictionary<TPlayer, Rating>();

        /// <summary>
        /// Constructs a new team.
        /// </summary>
        public Team()
        {
        }

        /// <summary>
        /// Constructs a <see cref="Team"/> and populates it with the specified <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <param name="rating">The rating of the <paramref name="player"/>.</param>
        public Team(TPlayer player, Rating rating)
        {
            AddPlayer(player, rating);
        }

        /// <summary>
        /// Adds the <paramref name="player"/> to the team.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <param name="rating">The rating of the <paramref name="player"/>.</param>
        /// <returns>The instance of the team (for chaining convenience).</returns>
        public Team<TPlayer> AddPlayer(TPlayer player, Rating rating)
        {
            _PlayerRatings[player] = rating;
            return this;
        }

        /// <summary>
        /// Returns the <see cref="Team"/> as a simple dictionary.
        /// </summary>
        /// <returns>The <see cref="Team"/> as a simple dictionary.</returns>
        public IDictionary<TPlayer, Rating> AsDictionary()
        {
            return _PlayerRatings;
        }
    }

    /// <summary>
    /// Helper class for working with a single team.
    /// </summary>
    public class Team : Team<Player>
    {
        /// <summary>
        /// Constructs a new team.
        /// </summary>
        public Team()
        {
        }

        /// <summary>
        /// Constructs a <see cref="Team"/> and populates it with the specified <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The player to add.</param>
        /// <param name="rating">The rating of the <paramref name="player"/>.</param>
        public Team(Player player, Rating rating)
            : base(player, rating)
        {
        }
    }

    /// <summary>
    /// Helper class for working with multiple teams.
    /// </summary>
    public static class Teams
    {
        /// <summary>
        /// Concatenates multiple teams into a list of teams.
        /// </summary>
        /// <param name="teams">The teams to concatenate together.</param>
        /// <returns>A sequence of teams.</returns>
        public static IEnumerable<IDictionary<TPlayer, Rating>> Concat<TPlayer>(params Team<TPlayer>[] teams)
        {
            return teams.Select(t => t.AsDictionary());
        }
    }
}