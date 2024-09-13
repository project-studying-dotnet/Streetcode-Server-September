using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.AdditionalContent.Tag.GetById;
using Streetcode.DAL.Repositories.Interfaces.Base;
using Xunit;
using FluentAssertions;
using System.Linq.Expressions;
using Streetcode.DAL.Entities.Streetcode.TextContent;

using tagEntity = Streetcode.DAL.Entities.AdditionalContent.Tag;

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
            var tagEntity = new tagEntity { Id = tagId, Title = "TestTag" };
            var tagDto = new TagDto { Id = tagId, Title = "TestTag" };
            var request = new GetTagByIdQuery(tagId);


            _repositoryWrapperMock.Setup(repo => repo.TagRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<tagEntity, bool>>>(exp => exp.Compile().Invoke(tagEntity)),
                null)).ReturnsAsync(tagEntity);

            _mapperMock.Setup(m => m.Map<TagDto>(tagEntity)).Returns(tagDto);

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(tagDto);

            _repositoryWrapperMock.Verify<Task<tagEntity>>(repo => repo.TagRepository.GetFirstOrDefaultAsync(
                It.Is<Expression<Func<tagEntity, bool>>>(exp => exp.Compile().Invoke(new tagEntity { Id = request.Id })),
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
                .GetFirstOrDefaultAsync(It.IsAny<Expression<Func<tagEntity, bool>>>(), null))
                .ReturnsAsync((tagEntity)null);

            // Act
            var result = await _handler.Handle(new GetTagByIdQuery(tagId), CancellationToken.None);

            // Assert
            result.IsFailed.Should().BeTrue();
            result.Errors.Should().ContainSingle(e => e.Message.Contains($"Cannot find a Tag with corresponding id: {tagId}"));

            _mapperMock.Verify(m => m.Map<TagDto>(It.IsAny<tagEntity>()), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), $"Cannot find a Tag with corresponding id: {tagId}"), Times.Once);
        }

        // Метод для проверки, что выражение использует правильный Id
        private bool CheckExpression(Expression<Func<tagEntity, bool>> expression, int expectedId)
        {
            // Преобразуем выражение в делегат
            var func = expression.Compile();
            // Создаём тестовый объект Tag с ожидаемым Id
            var testTag = new tagEntity { Id = expectedId };
            // Проверяем, что выражение возвращает true для этого объекта
            return func(testTag);
        }

    }
}
