using AutoMapper;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent.Tag;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetByStreetcodeId;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using tagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;

namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.Tag
{
    public class GetTagByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetTagByStreetcodeIdHandler _handler;

        public GetTagByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetTagByStreetcodeIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTags_WhenTagsExist()
        {
            // Arrange
            var streetcodeId = 1;
            var tagEntities = new List<StreetcodeTagIndex>
            {
                new StreetcodeTagIndex { StreetcodeId = streetcodeId, Tag = new tagEntity { Title = "Tag1" }, Index = 1 },
                new StreetcodeTagIndex { StreetcodeId = streetcodeId, Tag = new tagEntity { Title = "Tag2" }, Index = 2 }
            };

            var tagDtos = new List<StreetcodeTagDto>
            {
                new StreetcodeTagDto { Title = "Tag1", Index = 1 },
                new StreetcodeTagDto { Title = "Tag2", Index = 2 }
            };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeTagIndexRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                   It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
               .ReturnsAsync(tagEntities);

            _mapperMock.Setup(m => m.Map<IEnumerable<StreetcodeTagDto>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()))
                       .Returns(tagDtos);

            // Act
            var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(tagDtos, result.Value);

            _repositoryWrapperMock.Verify(repo => repo.StreetcodeTagIndexRepository.GetAllAsync(
                   It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                   It.Is<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>(include => include != null)), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<StreetcodeTagDto>>(tagEntities), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }


        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenTagsAreNull()
        {
            // Arrange
            var streetcodeId = 1;
            var errorMsg = $"Cannot find any tag by the streetcode id: {streetcodeId}";

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeTagIndexRepository
               .GetAllAsync(
                   It.IsAny<Expression<Func<StreetcodeTagIndex, bool>>>(),
                   It.IsAny<Func<IQueryable<StreetcodeTagIndex>, IIncludableQueryable<StreetcodeTagIndex, object>>>()))
               .ReturnsAsync((IEnumerable<StreetcodeTagIndex>)null);

            // Act
            var result = await _handler.Handle(new GetTagByStreetcodeIdQuery(streetcodeId), CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(errorMsg, result.Errors.First().Message);

            _mapperMock.Verify(m => m.Map<IEnumerable<StreetcodeTagDto>>(It.IsAny<IEnumerable<StreetcodeTagIndex>>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find any tag by the streetcode id: {streetcodeId}"), Times.Once);
        }
    }
}
