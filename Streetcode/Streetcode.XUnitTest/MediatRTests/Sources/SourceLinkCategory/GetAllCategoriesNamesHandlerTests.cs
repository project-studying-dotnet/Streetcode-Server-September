using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using SrcLinkCategory = Streetcode.DAL.Entities.Sources.SourceLinkCategory;


namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory
{
    public class GetAllCategoryNamesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllCategoryNamesHandler _handler;

        public GetAllCategoryNamesHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllCategoryNamesHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenSourceLinkCategoriesExist()
        {
            // Arrange
            var sourceLinkCategories = new List<SrcLinkCategory> { new SrcLinkCategory { Id = 1, Title = "Link Title", ImageId = 1 }};
            var mappedCategories = new List<CategoryWithNameDto> { new CategoryWithNameDto { Id = 1, Title = "Link Title" }};

            _repositoryWrapperMock.Setup(repo => repo.SourceCategoryRepository
                .GetAllAsync(null, null))
                .ReturnsAsync(sourceLinkCategories);

            _mapperMock.Setup(m => m.Map<IEnumerable<CategoryWithNameDto>>(sourceLinkCategories))
                .Returns(mappedCategories);

            // Act
            var result = await _handler.Handle(new GetAllCategoryNamesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(mappedCategories.Count, result.Value.Count());
        }

        [Fact]
        public async Task Handle_ReturnsFailed_WhenSourceLinkCategoriesAreNull()
        {
            // Arrange
            _repositoryWrapperMock.Setup(repo => repo.SourceCategoryRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<SrcLinkCategory, bool>>>(),
                    null))
                .ReturnsAsync((IEnumerable<SrcLinkCategory>) null);

            // Act
            var result = await _handler.Handle(new GetAllCategoryNamesQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal("Categories is null", result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<GetAllCategoryNamesQuery>(), "Categories is null"), Times.Once);
        }
    }
}
