using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.BLL.MediatR.Media.Art.GetByStreetcodeId;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class GetArtsByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly Mock<IBlobService> _blobServiceMock;
        private readonly GetArtsByStreetcodeIdHandler _handler;

        public GetArtsByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _blobServiceMock = new Mock<IBlobService>();
            _handler = new GetArtsByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object,
                                                        _blobServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenArtsDoExist()
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

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                                        It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                                        It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                                 .ReturnsAsync(arts);


            _mapperMock.Setup(m => m.Map<IEnumerable<ArtDto>>(arts)).Returns(artDTOs);

            //Act
            var result = await _handler.Handle(new GetArtsByStreetcodeIdQuery(It.IsAny<int>()), CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(artDTOs, result.Value);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenArtsDoNotExist()
        {
            //Arrange
            int streetcodeId = It.IsAny<int>(); 
            var error = $"Cannot find any art with corresponding streetcode id: {streetcodeId}";

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetAllAsync(
                                        It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                                        It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))!
                                 .ReturnsAsync((IEnumerable<ArtEntity>) null);

            //Act
            var result = await _handler.Handle(new GetArtsByStreetcodeIdQuery(It.IsAny<int>()), CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<GetArtsByStreetcodeIdQuery>(), error), Times.Once);
        }
    }
}
