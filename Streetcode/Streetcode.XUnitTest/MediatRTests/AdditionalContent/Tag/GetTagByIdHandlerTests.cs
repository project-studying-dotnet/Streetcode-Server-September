using AutoMapper;
using FluentResults;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.DAL.Entities.AdditionalContent;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using Streetcode.DAL.Entities.Streetcode.TextContent;
using Org.BouncyCastle.Asn1.Ocsp;
using Streetcode.BLL.MediatR.Streetcode.Term.GetById;

namespace Streetcode.XUnitTest.MediatRTests.AdditionalContent.Tag
{
    public class GetTagByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetTagByIdHandler _handler;

        public GetTagByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetTagByIdHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnTagDto_WhenTagExists()
        {
            // Arrange
            var tagId = 1; // Ожидаемый Id
            var tagEntity = new DAL.Entities.AdditionalContent.Tag { Id = tagId, Title = "TestTag" };
            var tagDto = new TagDto { Id = tagId, Title = "TestTag" };
            var request = new GetTagByIdQuery(tagId);


            _repositoryWrapperMock.Setup(repo => repo.TagRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(exp => exp.Compile().Invoke(tagEntity)),
                null)).ReturnsAsync(tagEntity);

            _mapperMock.Setup(m => m.Map<TagDto>(tagEntity)).Returns(tagDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDto);

            _repositoryWrapperMock.Verify<Task<DAL.Entities.AdditionalContent.Tag>>(repo => repo.TagRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(exp => exp.Compile().Invoke(new DAL.Entities.AdditionalContent.Tag { Id = request.Id })),
                null), Times.Once);

            _mapperMock.Verify(m => m.Map<TagDto>(tagEntity), Times.Once);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Handle_ShouldReturnFailResult_WhenTagIsNull()
        {
            // Arrange
            var tagId = 1;

            _repositoryWrapperMock.Setup(repo => repo.TagRepository
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>>>(), null))
                .ReturnsAsync((DAL.Entities.AdditionalContent.Tag)null);

            // Act
            var result = await _handler.Handle(new GetTagByIdQuery(tagId), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains($"Cannot find a Tag with corresponding id: {tagId}"));

            _mapperMock.Verify(m => m.Map<TagDto>(It.IsAny<DAL.Entities.AdditionalContent.Tag>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find a Tag with corresponding id: {tagId}"), Times.Once);
        }

        // Метод для проверки, что выражение использует правильный Id
        private bool CheckExpression(Expression<Func<DAL.Entities.AdditionalContent.Tag, bool>> expression, int expectedId)
        {
            // Преобразуем выражение в делегат
            var func = expression.Compile();
            // Создаём тестовый объект Tag с ожидаемым Id
            var testTag = new DAL.Entities.AdditionalContent.Tag { Id = expectedId };
            // Проверяем, что выражение возвращает true для этого объекта
            return func(testTag);
        }

    }
}
