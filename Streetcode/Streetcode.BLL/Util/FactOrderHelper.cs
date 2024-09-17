using Streetcode.DAL.Entities.Streetcode.TextContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Util
{
    public static class FactOrderHelper
    {
        public static void UpdateFactOrder(List<Fact> facts, int factId, int newSortOrder)
        {
            var factToMove = facts.FirstOrDefault(f => f.Id == factId) ?? throw new ArgumentException($"Fact with Id {factId} not found");
            
            var currentSortOrder = factToMove.SortOrder;

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

            factToMove.SortOrder = newSortOrder;
        }
    }
}
