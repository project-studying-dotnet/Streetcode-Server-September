using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.Create;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.Dto.AdditionalContent.Tag;

namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.Tag
{
    public class CreateTagHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateTagHandler _handler;

        public CreateTagHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateTagHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTagDto_WhenCreationIsSuccessful()
        {
            // Arrange
            var createTagQuery = new CreateTagQuery(new CreateTagDto { Title = "New Tag" });
            var newTagEntity = new DAL.Entities.AdditionalContent.Tag { Title = "New Tag" };
            var tagDto = new TagDto { Title = "New Tag" };

            _repositoryWrapperMock.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
                                  .ReturnsAsync(newTagEntity);

            _mapperMock.Setup(m => m.Map<TagDto>(newTagEntity)).Returns(tagDto);

            // Act
            var result = await _handler.Handle(createTagQuery, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDto);

            _repositoryWrapperMock.Verify(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenSaveThrowsException()
        {
            // Arrange
            var createTagQuery = new CreateTagQuery(new CreateTagDto { Title = "New Tag" });
            var newTagEntity = new DAL.Entities.AdditionalContent.Tag { Title = "New Tag" };

            _repositoryWrapperMock.Setup(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()))
                                  .ReturnsAsync(newTagEntity);

            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync())
                                  .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _handler.Handle(createTagQuery, CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains("Database error"));

            _repositoryWrapperMock.Verify(r => r.TagRepository.CreateAsync(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Once);
            _repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Once);
        }
    }
}
