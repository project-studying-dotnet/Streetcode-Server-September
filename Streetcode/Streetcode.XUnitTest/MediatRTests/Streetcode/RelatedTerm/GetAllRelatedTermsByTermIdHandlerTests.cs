using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Streetcode.BLL.Dto.Streetcode.TextContent.Term;
using Xunit;

using RelatedTermEntity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedTerm
{
    public class GetAllRelatedTermsByTermIdHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllRelatedTermsByTermIdHandler _handler;

        public GetAllRelatedTermsByTermIdHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllRelatedTermsByTermIdHandler(_mapperMock.Object, _repositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsRelatedTermsDTOs_WhenRelatedTermsExist()
        {
            // Arrange
            int id = 1;
            var query = new GetAllRelatedTermsByTermIdQuery(id);
            var relatedTerms = new List<RelatedTermEntity> { new RelatedTermEntity { TermId = id } };
            var relatedTermsDTOs = new List<RelatedTermDto> { new RelatedTermDto() };

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTermEntity>, IIncludableQueryable<RelatedTermEntity, object>>>()))
             .ReturnsAsync(relatedTerms);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDto>>(relatedTerms))
            .Returns(relatedTermsDTOs);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(relatedTermsDTOs);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenRelatedTermsNotFound()
        {
            // Arrange
            int id = 1;
            var query = new GetAllRelatedTermsByTermIdQuery(id);

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTermEntity>, IIncludableQueryable<RelatedTermEntity, object>>>()))
             .ReturnsAsync((IEnumerable<RelatedTermEntity>)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot get words by term id");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot get words by term id"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenRelatedTermsNotMappable()
        {
            // Arrange
            int id = 1;
            var query = new GetAllRelatedTermsByTermIdQuery(id);
            var relatedTerms = new List<RelatedTermEntity> { new RelatedTermEntity { TermId = id } };
            var relatedTermsDTOs = new List<RelatedTermDto> { new RelatedTermDto() };

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTermEntity>, IIncludableQueryable<RelatedTermEntity, object>>>()))
             .ReturnsAsync(relatedTerms);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDto>>(relatedTerms))
            .Returns((IEnumerable<RelatedTermDto>)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot create DTOs for related words!");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot create DTOs for related words!"), Times.Once);
        }
    }
}