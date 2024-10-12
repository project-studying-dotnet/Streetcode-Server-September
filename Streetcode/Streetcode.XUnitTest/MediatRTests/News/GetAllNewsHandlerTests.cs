using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Media.Images;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.GetAll;
using Streetcode.DAL.Entities.Media.Images;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using NewsEntity = Streetcode.DAL.Entities.News.News;

namespace Streetcode.XUnitTest.MediatRTests.News
{
    public class GetAllNewsHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IBlobAzureService> _blobAzureServiceMock;
        private readonly GetAllNewsHandler _handler;

        public GetAllNewsHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _blobAzureServiceMock = new Mock<IBlobAzureService>();
            _handler = new GetAllNewsHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _blobAzureServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsNewsDto_WhenNewsExist()
        {
            // Arrange
            var news = new List<NewsEntity>
            {
                new NewsEntity { Id = 1, Title = "Test News 1", Image = new Image { BlobName = "image1.jpg" } },
                new NewsEntity { Id = 2, Title = "Test News 2" }
            };
            var newsDtos = new List<NewsDto>
            {
                new NewsDto { Id = 1, Title = "Test News 1", Image = new ImageDto { BlobName = "image1.jpg" } },
                new NewsDto { Id = 2, Title = "Test News 2" }
            };

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                .GetAllAsync(
                    It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                    It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync(news);

            _mapperMock.Setup(mapper => mapper.Map<IEnumerable<NewsDto>>(news))
                .Returns(newsDtos);

            _blobAzureServiceMock
                .Setup(service => service.FindFileInStorageAsBase64("image1.jpg"))
                .Returns("base64string");

            // Act
            var result = await _handler.Handle(new GetAllNewsQuery(), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(newsDtos, result.Value);
        }


        [Fact]
        public async Task Handle_ReturnsErrorMsg_WhenNewsNotFound()
        {
            // Arrange
            const string expectedErrorMsg = "There are no news in the database";

            _repositoryWrapperMock.Setup(repo => repo.NewsRepository
                 .GetAllAsync(
                     It.IsAny<Expression<Func<NewsEntity, bool>>>(),
                     It.IsAny<Func<IQueryable<NewsEntity>, IIncludableQueryable<NewsEntity, object>>>()))
                .ReturnsAsync((IEnumerable<NewsEntity>)null);


            // Act and Assert
            var exception = await Assert.ThrowsAsync<CustomException>(async () =>
                await _handler.Handle(new GetAllNewsQuery(), CancellationToken.None));

            Assert.Equal(StatusCodes.Status404NotFound, exception.StatusCode);
            Assert.Equal(expectedErrorMsg, exception.Message);
        }
    }
}
