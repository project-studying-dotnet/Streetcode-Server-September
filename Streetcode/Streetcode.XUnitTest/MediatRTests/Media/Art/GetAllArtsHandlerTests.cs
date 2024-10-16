using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class GetAllArtsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllArtsHandler _handler;

        public GetAllArtsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllArtsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenArtsExist()
        {
            //Arrange
            var arts = new List<ArtEntity>()
            {
                new ArtEntity { Id = 1, Description = "Smth", Title = "Smh", ImageId = 1},
                new ArtEntity { Id = 2, Description = "Smth2", Title = "Smh2", ImageId = 2}
            };

            var artDTOs = new List<ArtDto>()
            {
                new ArtDto { Id = 1, Description = "Smth", Title = "Smh", ImageId = 1},
                new ArtDto { Id = 2, Description = "Smth2", Title = "Smh2", ImageId = 2}
            };

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                                            It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                                  .ReturnsAsync(arts);

            _mapperMock.Setup(m => m.Map<IEnumerable<ArtDto>>(arts)).Returns(artDTOs);

            //Act
            var result = await _handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(artDTOs.Count, result.Value.Count());
        }

        [Fact]
        public async Task Handle_ReturnFailResult_WhenArtsDoesNotExist()
        {
            //Arrange
            var error = $"Cannot find any arts";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(It.IsAny<Expression<Func<ArtEntity, bool>>>()!,
                                            It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))!
                                  .ReturnsAsync((IEnumerable<ArtEntity>)null);

            //Act
            var result = await _handler.Handle(new GetAllArtsQuery(), CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<GetAllArtsQuery>(), error), Times.Once);
        }
    }
}
