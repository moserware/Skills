
namespace Moserware.Skills.Elo
{
    // see http://ratings.fide.com/calculator_rtd.phtml for details
    public class FideKFactor : KFactor
    {
        public FideKFactor()
        {
        }

        public override double GetValueForRating(double rating)
        {
            if (rating < 2400)
            {
                return 15;
            }

            return 10;
        }

        /// <summary>
        /// Indicates someone who has played less than 30 games.
        /// </summary>        
        public class Provisional : FideKFactor
        {
            public Provisional()
            {                
            }

            public override double GetValueForRating(double rating)
            {
                return 25;
            }
        }
    }
}
