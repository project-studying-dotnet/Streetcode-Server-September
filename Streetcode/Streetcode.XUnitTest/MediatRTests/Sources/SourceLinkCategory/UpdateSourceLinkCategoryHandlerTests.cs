using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Sources;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using StreetcodeCategoryContentEnitity = Streetcode.DAL.Entities.Sources.StreetcodeCategoryContent;

namespace Streetcode.XUnitTest.MediatRTests.Sources.SourceLinkCategory
{
    public class UpdateSourceLinkCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdateStreetcodeCategoryContentHandler _handler;

        public UpdateSourceLinkCategoryHandlerTests()
        {
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();

            _handler = new UpdateStreetcodeCategoryContentHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_SuccessfulUpdate_ReturnsUpdatedContent()
        {
            // Arrange
            var existingEntity = new StreetcodeCategoryContentEnitity { SourceLinkCategoryId = 1, StreetcodeId = 1, Text = "Original Text" };
            var updateDto = new SourceLinkCategoryContentUpdateDto { SourceLinkCategoryId = 2, StreetcodeId = 1, Text = "Updated Text" };
            var command = new UpdateStreetcodeCategoryContentCommand(2, updateDto);

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                 .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContentEnitity, bool>>>(), null))
              .ReturnsAsync(existingEntity);

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository.Update(It.IsAny<StreetcodeCategoryContentEnitity>()));
            _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<SourceLinkCategoryContentUpdateDto>(It.IsAny<StreetcodeCategoryContentEnitity>()))
                         .Returns(updateDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(updateDto.Text, result.Value.Text);
            Assert.Equal(updateDto.SourceLinkCategoryId, result.Value.SourceLinkCategoryId);

            _repositoryMock.Verify(repo => repo.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsFailResult()
        {
            // Arrange
            var command = new UpdateStreetcodeCategoryContentCommand(2, new SourceLinkCategoryContentUpdateDto { Text = "Updated Text" });

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                 .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContentEnitity, bool>>>(), null))
              .ReturnsAsync((StreetcodeCategoryContentEnitity)null);

            //Act
            var result = await _handler.Handle(command, CancellationToken.None);

            //Assert 
            Assert.False(result.IsSuccess);
            Assert.Equal("Cannot convert null to category content", result.Errors.FirstOrDefault().Message);

            _loggerMock.Verify(logger => logger.LogError(command, "Cannot convert null to category content"), Times.Once);
            _repositoryMock.Verify(repo => repo.StreetcodeCategoryContentRepository
                           .Update(It.IsAny<StreetcodeCategoryContentEnitity>()), Times.Never);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task Handle_SaveChangesFails_ReturnsFailResult()
        {
            // Arrange
            var existingEntity = new StreetcodeCategoryContentEnitity { SourceLinkCategoryId = 1, StreetcodeId = 1, Text = "Original Text" };
            var updateDto = new SourceLinkCategoryContentUpdateDto { SourceLinkCategoryId = 2, StreetcodeId = 1, Text = "Updated Text" };
            var command = new UpdateStreetcodeCategoryContentCommand(2, updateDto);

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository
                 .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<StreetcodeCategoryContentEnitity, bool>>>(), null))
              .ReturnsAsync(existingEntity);

            _repositoryMock.Setup(r => r.StreetcodeCategoryContentRepository.Update(It.IsAny<StreetcodeCategoryContentEnitity>()));
            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0); 

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Failed to update category content", result.Errors.FirstOrDefault().Message);

            _loggerMock.Verify(logger => logger.LogError(command, "Failed to update category content"), Times.Once);
            _repositoryMock.Verify(repo => repo.StreetcodeCategoryContentRepository.Update(existingEntity), Times.Once);
            _repositoryMock.Verify(repo => repo.SaveChangesAsync(), Times.Once);
        }
    }
}
