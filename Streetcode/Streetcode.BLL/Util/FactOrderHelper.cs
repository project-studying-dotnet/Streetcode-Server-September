using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Entities.Streetcode.TextContent;

namespace Streetcode.BLL.Util
{
    public static class FactOrderHelper
    {
        public static void UpdateFactOrder(List<Fact> facts, int factId, int newSortOrder = int.MaxValue)
        {
            // Find the fact to move.
            var factToMove = facts.FirstOrDefault(f => f.Id == factId) ?? 
                throw new CustomException($"Fact with Id {factId} not found", StatusCodes.Status204NoContent);

            var currentSortOrder = factToMove.SortOrder;

            // Checking the correctness of the new SortOrder
            var maxOrderInFacts = facts.Select(f => f.SortOrder).Max();

            if (newSortOrder > maxOrderInFacts) newSortOrder = maxOrderInFacts;

            if (newSortOrder < 1) newSortOrder = 1;

            // If the new order is the same as the old order - nothing needs to change
            if (currentSortOrder == newSortOrder)
                return;

            if (newSortOrder > currentSortOrder)
            {
                // Shifting down the facts between the old order and the new order
                var factsToShiftDown = facts
                    .Where(f => f.SortOrder > currentSortOrder && f.SortOrder <= newSortOrder)
                    .OrderBy(f => f.SortOrder);

                foreach (var fact in factsToShiftDown)
                {
                    fact.SortOrder--;
                }
            }
            else
            {
                // Shifting up the facts between the new order and the old order
                var factsToShiftUp = facts
                    .Where(f => f.SortOrder >= newSortOrder && f.SortOrder < currentSortOrder)
                    .OrderBy(f => f.SortOrder);

                foreach (var fact in factsToShiftUp)
                {
                    fact.SortOrder++;
                }
            }

            // Move the fact to a new position
            factToMove.SortOrder = newSortOrder;
        }
    }
}
