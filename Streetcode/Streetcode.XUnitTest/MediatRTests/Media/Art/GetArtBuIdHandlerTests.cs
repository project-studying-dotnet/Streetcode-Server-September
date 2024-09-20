using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Org.BouncyCastle.Asn1.Crmf;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.Dto.Media.Art;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Media.Art.GetAll;
using Streetcode.BLL.MediatR.Media.Art.GetById;
using Streetcode.BLL.MediatR.Streetcode.Fact.GetById;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using ArtEntity = Streetcode.DAL.Entities.Media.Images.Art;

namespace Streetcode.XUnitTest.MediatRTests.Media.Art
{
    public class GetArtBuIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetArtByIdHandler _handler;

        public GetArtBuIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetArtByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnOkResult_WhenArtsExist()
        {
            //Arrange
            var artId = 1;
            var request = new GetArtByIdQuery(artId);
            var art = new ArtEntity { Id = artId, Description = "Smth", Title = "Smh", ImageId = 1 };
            var artDTO = new ArtDto { Id = artId, Description = "Smth", Title = "Smh", ImageId = 1 };

            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetFirstOrDefaultAsync(
                                         It.Is<Expression<Func<ArtEntity, bool>>>(exp => exp.Compile().Invoke(art)),
                                         It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                                  .ReturnsAsync(art);

            _mapperMock.Setup(m => m.Map<ArtDto>(art)).Returns(artDTO);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.True(result.IsSuccess);
            Assert.Equal(artDTO, result.Value);
            _repositoryWrapperMock.Verify(repo => repo.ArtRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<ArtEntity, bool>>>(
                    exp => exp.Compile().Invoke(new ArtEntity { Id = art.Id })), null), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnFailResult_WhenArtsDoNotExist()
        {
            var artId = 1;
            var request = new GetArtByIdQuery(artId);
            var error = $"Cannot find an art with corresponding id: {artId}";

            //Arrange
            _repositoryWrapperMock.Setup(repo => repo.ArtRepository.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<ArtEntity, bool>>>(),
                                        It.IsAny<Func<IQueryable<ArtEntity>, IIncludableQueryable<ArtEntity, object>>>()))
                                 .ReturnsAsync((ArtEntity) null);

            //Act
            var result = await _handler.Handle(request, CancellationToken.None);

            //Assert
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal(error, result.Errors.First().Message);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<GetArtByIdQuery>(), error), Times.Once);
        }
    }
}
