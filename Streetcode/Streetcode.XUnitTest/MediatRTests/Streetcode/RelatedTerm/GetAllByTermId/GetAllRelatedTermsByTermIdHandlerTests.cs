using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.DTO.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.GetAllByTermId;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.StreetcodeTests.RelatedTermTests
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
            var relatedTerms = new List<RelatedTerm> { new RelatedTerm { TermId = id } };
            var relatedTermsDTOs = new List<RelatedTermDTO> { new RelatedTermDTO() };

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()))
             .ReturnsAsync(relatedTerms);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDTO>>(relatedTerms))
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
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()))
             .ReturnsAsync((IEnumerable<RelatedTerm>)null);

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
            var relatedTerms = new List<RelatedTerm> { new RelatedTerm { TermId = id } };
            var relatedTermsDTOs = new List<RelatedTermDTO> { new RelatedTermDTO() };

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(),
                It.IsAny<Func<IQueryable<RelatedTerm>, IIncludableQueryable<RelatedTerm, object>>>()))
             .ReturnsAsync(relatedTerms);

            _mapperMock.Setup(m => m.Map<IEnumerable<RelatedTermDTO>>(relatedTerms))
            .Returns((IEnumerable<RelatedTermDTO>)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot create DTOs for related words!");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot create DTOs for related words!"), Times.Once);
        }
    }
}
