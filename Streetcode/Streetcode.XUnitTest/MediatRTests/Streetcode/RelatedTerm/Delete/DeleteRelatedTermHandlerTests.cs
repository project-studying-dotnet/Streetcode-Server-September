using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Delete;
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
    public class DeleteRelatedTermHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly DeleteRelatedTermHandler _handler;

        public DeleteRelatedTermHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new DeleteRelatedTermHandler(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenDTOFormed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new DeleteRelatedTermCommand(It.IsAny<string>());
            var relatedTerms = new List<RelatedTerm>(); 
            var entity = new RelatedTerm();

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(), null))
                .ReturnsAsync(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<RelatedTermDto>(entity))
                .Returns(relatedTermsDTO);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenRelatedTermNotFound()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new DeleteRelatedTermCommand(It.IsAny<string>());
            var relatedTerms = new List<RelatedTerm>();
            var entity = new RelatedTerm();

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(), null))
                .ReturnsAsync((RelatedTerm)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Cannot find a related term: {query.word}");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find a related term: {query.word}"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSaveFailed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new DeleteRelatedTermCommand(It.IsAny<string>());
            var relatedTerms = new List<RelatedTerm>();
            var entity = new RelatedTerm();

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<RelatedTerm, bool>>>(), null))
                .ReturnsAsync(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                        .ReturnsAsync(0);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Failed to delete a related term");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Failed to delete a related term"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenMappingFailed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new DeleteRelatedTermCommand(It.IsAny<string>());
            var relatedTerms = new List<RelatedTerm>();
            var entity = new RelatedTerm();

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetFirstOrDefaultAsync(
                 It.IsAny<Expression<Func<RelatedTerm, bool>>>(), null))
                 .ReturnsAsync(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                        .ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<RelatedTermDto>(entity))
                .Returns((RelatedTermDto) null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == $"Failed to delete a related term");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Failed to delete a related term"), Times.Once);
        }
    }
}
