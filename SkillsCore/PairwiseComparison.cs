namespace Moserware.Skills
{
    /// <summary>
    /// Represents a comparison between two players.
    /// </summary>
    /// <remarks>
    /// The actual values for the enum were chosen so that the also correspond to the multiplier for updates to means.
    /// </remarks>
    public enum PairwiseComparison
    {
        Win = 1,
        Draw = 0,
        Lose = -1
    }
}