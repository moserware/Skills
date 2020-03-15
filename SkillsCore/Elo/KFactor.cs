
namespace Moserware.Skills.Elo
{
    public class KFactor
    {
        private double _Value;

        protected KFactor()
        {
        }

        public KFactor(double exactKFactor)
        {
            _Value = exactKFactor;
        }

        public virtual double GetValueForRating(double rating)
        {
            return _Value;
        }
    }
}
