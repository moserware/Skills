using System.Collections.Generic;
using System.Linq;

namespace Moserware.Skills
{
    /// <summary>
    /// Helper class to sort ranks in non-decreasing order.
    /// </summary>
    internal static class RankSorter
    {
        /// <summary>
        /// Performs an in-place sort of the <paramref name="items"/> in according to the <paramref name="ranks"/> in non-decreasing order.
        /// </summary>
        /// <typeparam name="T">The types of items to sort.</typeparam>
        /// <param name="items">The items to sort according to the order specified by <paramref name="ranks"/>.</param>
        /// <param name="ranks">The ranks for each item where 1 is first place.</param>
        public static void Sort<T>(ref IEnumerable<T> teams, ref int[] teamRanks)
        {
            Guard.ArgumentNotNull(teams, "teams");
            Guard.ArgumentNotNull(teamRanks, "teamRanks");

            int lastObserverdRank = 0;
            bool needToSort = false;

            foreach (int currentRank in teamRanks)
            {
                // We're expecting ranks to go up (e.g. 1, 2, 2, 3, ...)
                // If it goes down, then we've got to sort it.
                if (currentRank < lastObserverdRank)
                {
                    needToSort = true;
                    break;
                }

                lastObserverdRank = currentRank;
            }

            if (!needToSort)
            {
                // Don't bother doing more work, it's already in a good order
                return;
            }

            // Get the existing items as an indexable list.
            List<T> itemsInList = teams.ToList();

            // item -> rank
            var itemToRank = new Dictionary<T, int>();

            for (int i = 0; i < itemsInList.Count; i++)
            {
                T currentItem = itemsInList[i];
                int currentItemRank = teamRanks[i];
                itemToRank[currentItem] = currentItemRank;
            }

            // Now we need a place for our results...
            var sortedItems = new T[teamRanks.Length];
            var sortedRanks = new int[teamRanks.Length];

            // where are we in the result?
            int currentIndex = 0;

            // Let LINQ-to-Objects to the actual sorting
            foreach (var sortedKeyValuePair in itemToRank.OrderBy(pair => pair.Value))
            {
                sortedItems[currentIndex] = sortedKeyValuePair.Key;
                sortedRanks[currentIndex++] = sortedKeyValuePair.Value;
            }

            // And we're done
            teams = sortedItems;
            teamRanks = sortedRanks;
        }
    }
}