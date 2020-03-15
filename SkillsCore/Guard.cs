using System;

namespace Moserware.Skills
{
    /// <summary>
    /// Verifies argument contracts.
    /// </summary>
    /// <remarks>These are used until .NET 4.0 ships with Contracts. For more information, 
    /// see http://www.moserware.com/2008/01/borrowing-ideas-from-3-interesting.html</remarks>
    internal static class Guard
    {
        public static void ArgumentNotNull(object value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        public static void ArgumentIsValidIndex(int index, int count, string parameterName)
        {
            if ((index < 0) || (index >= count))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        public static void ArgumentInRangeInclusive(double value, double min, double max, string parameterName)
        {
            if ((value < min) || (value > max))
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}