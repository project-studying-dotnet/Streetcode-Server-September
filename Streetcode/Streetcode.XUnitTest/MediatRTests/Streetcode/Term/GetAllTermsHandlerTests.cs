using Xunit;
using Moq;
using AutoMapper;
using Streetcode.BLL.MediatR.Streetcode.Term.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

using TermEntity = Streetcode.DAL.Entities.Streetcode.TextContent.Term;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.Term 
{ 
    public class GetAllTermsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _mockRepositoryWrapper;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILoggerService> _mockLogger;
        private readonly GetAllTermsHandler _handler;

        public GetAllTermsHandlerTests()
        {
            _mockRepositoryWrapper = new Mock<IRepositoryWrapper>();
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILoggerService>();
            _handler = new GetAllTermsHandler(_mockRepositoryWrapper.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Handle_ReturnsTerms_WhenTermsExist()
        {
            // Arrange
            var terms = new List<TermEntity> { new TermEntity { Id = 1, Title = "Test Term" } };
            _mockRepositoryWrapper.Setup(
                repo => repo.TermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<TermEntity, bool>>>(), It.IsAny<Func<IQueryable<TermEntity>, IIncludableQueryable<TermEntity, object>>>()))
            .ReturnsAsync(terms);


            var termDtos = new List<TermDto> { new TermDto { Id = 1, Title = "Test Term" } };
            _mockMapper.Setup(mapper => mapper.Map<IEnumerable<TermDto>>(terms))
                .Returns(termDtos);

            // Act
            var result = await _handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(terms.Count, result.Value.Count());
        }

        [Fact]
        public async Task Handle_ReturnsFail_WhenTermsNotFound()
        {
            // Arrange
            _mockRepositoryWrapper.Setup(
                repo => repo.TermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<TermEntity, bool>>>(), It.IsAny<Func<IQueryable<TermEntity>, IIncludableQueryable<TermEntity, object>>>()))
                .ReturnsAsync((IEnumerable<TermEntity>)null);

            // Act
            var result = await _handler.Handle(new GetAllTermsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal("Cannot find any term", result.Errors.First().Message);
            _mockLogger.Verify(logger => logger.LogError(It.IsAny<GetAllTermsQuery>(), "Cannot find any term"), Times.Once);
        }
    }
}