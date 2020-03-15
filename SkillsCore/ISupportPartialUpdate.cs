namespace Moserware.Skills
{
    public interface ISupportPartialUpdate
    {
        /// <summary>
        /// Indicated how much of a skill update a player should receive where 0.0 represents no update and 1.0 represents 100% of the update.
        /// </summary>
        double PartialUpdatePercentage { get; }
    }
}