namespace Moserware.Skills
{
    /// <summary>
    /// Indicates support for allowing partial play (where a player only plays a part of the time).
    /// </summary>
    public interface ISupportPartialPlay
    {
        /// <summary>
        /// Indicates the percent of the time the player should be weighted where 0.0 indicates the player didn't play and 1.0 indicates the player played 100% of the time.
        /// </summary>        
        double PartialPlayPercentage { get; }
    }
}