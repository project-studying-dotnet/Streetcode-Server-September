using AutoMapper;
using Microsoft.AspNetCore.Http;
using Moq;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.MediatR.Media.Art.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;
using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;
using StreetcodeEntity = Streetcode.DAL.Entities.Streetcode.StreetcodeContent;

namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class CreateArtHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly CreateArtHandler _handler;

        public CreateArtHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _handler = new CreateArtHandler(_mapperMock.Object, _repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenArtIsCreatedSuccessfully()
        {
            // Arrange
            var artDto = new ArtCreateDto
            {
                Id = 1,
                Title = "New Art",
                ImageId = 1,
                Streetcodes = new List<StreetcodeShortDto>()
                {
                    new StreetcodeShortDto() { Id = 99 }
                }
            };

            var artEntity = new ArtEntity
            {
                Id = 1,
                Title = "New Art",
                ImageId = 1
            };
            var arts = new List<ArtEntity>() { new ArtEntity() { ImageId = 2 } };
            var streetcode = new StreetcodeEntity { Id = 99 };

            var command = new CreateArtCommand(artDto);

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(), null)).ReturnsAsync(arts);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null)).ReturnsAsync(streetcode);

            _mapperMock.Setup(m => m.Map<ArtEntity>(artDto)).Returns(artEntity);
            _repositoryWrapperMock.Setup(r => r.ArtRepository.CreateAsync(artEntity)).ReturnsAsync(artEntity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            _mapperMock.Setup(m => m.Map<ArtCreateDto>(artEntity)).Returns(artDto);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(artEntity.Id, result.Value.Id);
            Assert.Equal(artEntity.Title, result.Value.Title);
            Assert.Equal(artEntity.ImageId, result.Value.ImageId);
            Assert.Equal(streetcode.Id, result.Value.Streetcodes![0].Id);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenArtWithImageAlreadyExists()
        {
            // Arrange
            var artDto = new ArtCreateDto { ImageId = 1 };
            var arts = new List<ArtEntity>() { new ArtEntity() { ImageId = 1 } };

            var command = new CreateArtCommand(artDto);
            string errorMsg = $"An art with Image Id: {artDto.ImageId} already exists. " +
                               "Please choose a different image.";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(), null)).ReturnsAsync(arts);

            _mapperMock.Setup(m => m.Map<ArtEntity>(command.newArt)).Returns((ArtEntity)null!);

            // Act
            var exception = await Assert.ThrowsAsync<CustomException>(
                () => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenArtIsNull()
        {
            // Arrange
            var artDto = new ArtCreateDto();
            var arts = new List<ArtEntity>() { new ArtEntity() { ImageId = 1 } };

            var command = new CreateArtCommand(artDto);
            string errorMsg = "Cannot convert null to an art.";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(), null)).ReturnsAsync(arts);

            _mapperMock.Setup(m => m.Map<ArtEntity>(command.newArt)).Returns((ArtEntity)null!);

            // Act
            var exception = await Assert.ThrowsAsync<CustomException>(
                () => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status204NoContent, exception.StatusCode);
        }

        [Fact]
        public async Task Handle_ShouldThrowCustomException_WhenSaveChangesFails()
        {
            // Arrange
            var artDto = new ArtCreateDto ();
            var artEntity = new ArtEntity ();
            var arts = new List<ArtEntity>() { new ArtEntity() { ImageId = 1 } };
            var streetcode = new StreetcodeEntity { Id = 99 };

            var command = new CreateArtCommand(artDto);
            string errorMsg = "Failed to save an art";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                It.IsAny<Expression<Func<ArtEntity, bool>>>(), null)).ReturnsAsync(arts);

            _repositoryWrapperMock.Setup(r => r.StreetcodeRepository.GetFirstOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeEntity, bool>>>(), null)).ReturnsAsync(streetcode);

            _mapperMock.Setup(m => m.Map<ArtEntity>(artDto)).Returns(artEntity);
            _repositoryWrapperMock.Setup(r => r.ArtRepository.CreateAsync(artEntity)).ReturnsAsync(artEntity);
            _repositoryWrapperMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var exception = await Assert.ThrowsAsync<CustomException>(
                () => _handler.Handle(command, CancellationToken.None));

            // Assert
            Assert.Equal(errorMsg, exception.Message);
            Assert.Equal(StatusCodes.Status400BadRequest, exception.StatusCode);
        }
    }
}
