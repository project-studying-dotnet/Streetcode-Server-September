using Xunit;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.BLL.Util;

namespace Streetcode.XUnitTest.Util;

public class FactOrderHelperTests
{
    [Fact]
    public void UpdateFactOrder_MoveFactDown_UpdatesSortOrderCorrectly()
    {
        // Arrange
        var factIdToMove = 1;
        var newSortOrder = 3;

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act
        FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder);

        // Assert
        Assert.Equal(3, facts.First(f => f.Id == factIdToMove).SortOrder);
        Assert.Equal(1, facts.First(f => f.Id == 2).SortOrder);
        Assert.Equal(2, facts.First(f => f.Id == 3).SortOrder);
    }

    [Fact]
    public void UpdateFactOrder_MoveFactUp_UpdatesSortOrderCorrectly()
    {
        // Arrange
        var factIdToMove = 3;
        var newSortOrder = 1;

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act
        FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder);

        // Assert
        Assert.Equal(1, facts.First(f => f.Id == factIdToMove).SortOrder);
        Assert.Equal(2, facts.First(f => f.Id == 1).SortOrder);
        Assert.Equal(3, facts.First(f => f.Id == 2).SortOrder);
    }

    [Fact]
    public void UpdateFactOrder_MoveFactToSamePosition_NoChanges()
    {
        // Arrange
        var factIdToMove = 2;
        var newSortOrder = 2;

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act
        FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder);

        // Assert
        Assert.Equal(1, facts.First(f => f.Id == 1).SortOrder);
        Assert.Equal(2, facts.First(f => f.Id == factIdToMove).SortOrder);
        Assert.Equal(3, facts.First(f => f.Id == 3).SortOrder);
    }

    [Fact]
    public void UpdateFactOrder_FactIdNotFound_ThrowsArgumentException()
    {
        // Arrange
        var factIdToMove = 4; // Does not exist
        var newSortOrder = 1;

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder));

        Assert.Equal($"Fact with Id {factIdToMove} not found", exception.Message);
    }

    [Fact]
    public void UpdateFactOrder_NewSortOrderExceedsMax_AdjustsToMax()
    {
        // Arrange
        var factIdToMove = 1;
        var newSortOrder = 5; // Exceeds max sort order

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act
        FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder);

        // Assert
        Assert.Equal(3, facts.First(f => f.Id == factIdToMove).SortOrder);
        Assert.Equal(1, facts.First(f => f.Id == 2).SortOrder);
        Assert.Equal(2, facts.First(f => f.Id == 3).SortOrder);
    }

    [Fact]
    public void UpdateFactOrder_NewSortOrderLessThanOne_AdjustsToOne()
    {
        // Arrange
        var factIdToMove = 3;
        var newSortOrder = 0; // Less than 1

        var facts = new List<Fact>
        {
            new Fact { Id = 1, SortOrder = 1 },
            new Fact { Id = 2, SortOrder = 2 },
            new Fact { Id = 3, SortOrder = 3 }
        };

        // Act
        FactOrderHelper.UpdateFactOrder(facts, factIdToMove, newSortOrder);

        // Assert
        Assert.Equal(1, facts.First(f => f.Id == factIdToMove).SortOrder);
        Assert.Equal(2, facts.First(f => f.Id == 1).SortOrder);
        Assert.Equal(3, facts.First(f => f.Id == 2).SortOrder);
    }
}
