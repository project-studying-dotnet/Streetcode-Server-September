using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetAll;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using FluentAssertions;
using Streetcode.DAL.Specification.AdditionalContent.TagSpecification;

using tagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;


namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.Tag
{
    public class GetAllTagsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllTagsHandler _handler;

        public GetAllTagsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllTagsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTags_WhenTagsExist()
        {
            // Arrange
            var tagEntities = new List<tagEntity>
            {
                new tagEntity { Title = "Tag1" },
                new tagEntity { Title = "Tag2" }
            };

            var tagDtos = new List<TagDto>
            {
                new TagDto { Title = "Tag1" },
                new TagDto { Title = "Tag2" }
            };

            _repositoryWrapperMock.Setup(r => r.TagRepository.GetItemsBySpecAsync(It.IsAny<GetAllTagsSpec>()))
                                  .ReturnsAsync(tagEntities);
            _mapperMock.Setup(m => m.Map<IEnumerable<TagDto>>(tagEntities)).Returns(tagDtos);

            // Act
            var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDtos);

            _repositoryWrapperMock.Verify(r => r.TagRepository.GetItemsBySpecAsync(It.IsAny<GetAllTagsSpec>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<TagDto>>(tagEntities), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenTagsAreNull()
        {
            // Arrange
            _repositoryWrapperMock.Setup(r => r.TagRepository.GetItemsBySpecAsync(It.IsAny<GetAllTagsSpec>()))
                                  .ReturnsAsync((IEnumerable<tagEntity>)null!);

            // Act
            var result = await _handler.Handle(new GetAllTagsQuery(), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains("Cannot find any tags"));

            _repositoryWrapperMock.Verify(r => r.TagRepository.GetItemsBySpecAsync(It.IsAny<GetAllTagsSpec>()), Times.Once);
            _mapperMock.Verify(m => m.Map<IEnumerable<TagDto>>(It.IsAny<IEnumerable<tagEntity>>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot find any tags"), Times.Once);
        }
    }
}
