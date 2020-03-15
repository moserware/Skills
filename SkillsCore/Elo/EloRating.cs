
namespace Moserware.Skills.Elo
{
    /// <summary>
    /// An Elo rating represented by a single number (mean).
    /// </summary>
    public class EloRating : Rating
    {
        public EloRating(double rating)
            : base(rating, 0)
        {
        }
    }
}
