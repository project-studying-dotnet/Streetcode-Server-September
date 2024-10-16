﻿using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetTagByTitle;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using FluentAssertions;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Specification.AdditionalContent.TagSpecification;

using tagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;


namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.Tag
{
    public class GetTagByTitleHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetTagByTitleHandler _handler;

        public GetTagByTitleHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetTagByTitleHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTagDto_WhenTagExists()
        {
            // Arrange
            var tagTitle = "TestTag";
            var tagEntity = new tagEntity { Title = tagTitle, Id = 1 };
            var tagDto = new TagDto { Title = tagTitle, Id = 1 };

            _repositoryWrapperMock.Setup(r => r.TagRepository.GetItemBySpecAsync(It.IsAny<GetTagByTitleSpec>()))
                                  .ReturnsAsync(tagEntity);

            _mapperMock.Setup(m => m.Map<TagDto>(tagEntity)).Returns(tagDto);

            // Act
            var result = await _handler.Handle(new GetTagByTitleQuery(tagTitle), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDto);

            _repositoryWrapperMock.Verify(r => r.TagRepository.GetItemBySpecAsync(It.IsAny<GetTagByTitleSpec>()), Times.Once);
            _mapperMock.Verify(m => m.Map<TagDto>(tagEntity), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenTagIsNull()
        {
            // Arrange
            var tagTitle = "NonExistentTag";
            var errorMsg = $"Cannot find any tag by the title: {tagTitle}";

            _repositoryWrapperMock.Setup(r => r.TagRepository.GetItemBySpecAsync(It.IsAny<GetTagByTitleSpec>()))
                                  .ReturnsAsync((tagEntity)null!);

            // Act
            var result = await _handler.Handle(new GetTagByTitleQuery(tagTitle), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains(errorMsg));

            _repositoryWrapperMock.Verify(r => r.TagRepository.GetItemBySpecAsync(It.IsAny<GetTagByTitleSpec>()), Times.Once);
            _mapperMock.Verify(m => m.Map<TagDto>(It.IsAny<tagEntity>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }
    }
}
