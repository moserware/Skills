namespace Moserware.Skills
{
    /// <summary>
    /// Represents a player who has a <see cref="Rating"/>.
    /// </summary>
    public class Player<T> : ISupportPartialPlay, ISupportPartialUpdate
    {
        private const double DefaultPartialPlayPercentage = 1.0; // = 100% play time
        private const double DefaultPartialUpdatePercentage = 1.0; // = receive 100% update
        private readonly T _Id;
        private readonly double _PartialPlayPercentage;

        private readonly double _PartialUpdatePercentage;

        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        public Player(T id)
            : this(id, DefaultPartialPlayPercentage, DefaultPartialUpdatePercentage)
        {
        }

        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        /// <param name="partialPlayPercentage">The weight percentage to give this player when calculating a new rank.</param>        
        public Player(T id, double partialPlayPercentage)
            : this(id, partialPlayPercentage, DefaultPartialUpdatePercentage)
        {
        }

        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        /// <param name="partialPlayPercentage">The weight percentage to give this player when calculating a new rank.</param>
        /// <param name="partialUpdatePercentage">/// Indicated how much of a skill update a player should receive where 0 represents no update and 1.0 represents 100% of the update.</param>
        public Player(T id, double partialPlayPercentage, double partialUpdatePercentage)
        {
            // If they don't want to give a player an id, that's ok...
            Guard.ArgumentInRangeInclusive(partialPlayPercentage, 0, 1.0, "partialPlayPercentage");
            Guard.ArgumentInRangeInclusive(partialUpdatePercentage, 0, 1.0, "partialUpdatePercentage");
            _Id = id;
            _PartialPlayPercentage = partialPlayPercentage;
            _PartialUpdatePercentage = partialUpdatePercentage;
        }

        /// <summary>
        /// The identifier for the player, such as a name.
        /// </summary>
        public T Id
        {
            get { return _Id; }
        }

        #region ISupportPartialPlay Members

        /// <summary>
        /// Indicates the percent of the time the player should be weighted where 0.0 indicates the player didn't play and 1.0 indicates the player played 100% of the time.
        /// </summary>        
        public double PartialPlayPercentage
        {
            get { return _PartialPlayPercentage; }
        }

        #endregion

        #region ISupportPartialUpdate Members

        /// <summary>
        /// Indicated how much of a skill update a player should receive where 0.0 represents no update and 1.0 represents 100% of the update.
        /// </summary>
        public double PartialUpdatePercentage
        {
            get { return _PartialUpdatePercentage; }
        }

        #endregion

        public override string ToString()
        {
            if (Id != null)
            {
                return Id.ToString();
            }

            return base.ToString();
        }
    }

    /// <summary>
    /// Represents a player who has a <see cref="Rating"/>.
    /// </summary>
    public class Player : Player<object>
    {
        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        public Player(object id)
            : base(id)
        {
        }

        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        /// <param name="partialPlayPercentage">The weight percentage to give this player when calculating a new rank.</param>
        /// <param name="partialUpdatePercentage">Indicated how much of a skill update a player should receive where 0 represents no update and 1.0 represents 100% of the update.</param>
        public Player(object id, double partialPlayPercentage)
            : base(id, partialPlayPercentage)
        {
        }

        /// <summary>
        /// Constructs a player.
        /// </summary>
        /// <param name="id">The identifier for the player, such as a name.</param>
        /// <param name="partialPlayPercentage">The weight percentage to give this player when calculating a new rank.</param>
        /// <param name="partialUpdatePercentage">Indicated how much of a skill update a player should receive where 0 represents no update and 1.0 represents 100% of the update.</param>
        public Player(object id, double partialPlayPercentage, double partialUpdatePercentage)
            : base(id, partialPlayPercentage, partialUpdatePercentage)
        {
        }
    }
}