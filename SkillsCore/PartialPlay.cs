namespace Moserware.Skills
{
    internal static class PartialPlay
    {
        public static double GetPartialPlayPercentage(object player)
        {
            // If the player doesn't support the interface, assume 1.0 == 100%
            var partialPlay = player as ISupportPartialPlay;
            if (partialPlay == null)
            {
                return 1.0;
            }

            double partialPlayPercentage = partialPlay.PartialPlayPercentage;

            // HACK to get around bug near 0
            const double smallestPercentage = 0.0001;
            if (partialPlayPercentage < smallestPercentage)
            {
                partialPlayPercentage = smallestPercentage;
            }

            return partialPlayPercentage;
        }
    }
}