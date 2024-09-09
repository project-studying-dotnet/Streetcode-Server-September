using AutoMapper;
using FluentAssertions;
using Moq;
using Streetcode.BLL.Dto.Streetcode.TextContent;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using RelatedTermEntity = Streetcode.DAL.Entities.Streetcode.TextContent.RelatedTerm;

namespace Streetcode.XUnitTest.MediatRTests.Streetcode.RelatedTerm
{
    public class CreateRelatedTermHandlerTests
    {
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IRepositoryWrapper> _repositoryMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreateRelatedTermHandler _handler;

        public CreateRelatedTermHandlerTests()
        {
            _mapperMock = new Mock<IMapper>();
            _repositoryMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreateRelatedTermHandler(_repositoryMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_CreatesRelatedTermsDTO_WhenDTOFormed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new CreateRelatedTermCommand(relatedTermsDTO);
            var relatedTerms = new List<RelatedTermEntity>();
            var entity = new RelatedTermEntity();

            _mapperMock.Setup(m => m.Map<RelatedTermEntity>(query.RelatedTerm))  
                .Returns(entity);

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
                .ReturnsAsync(relatedTerms);

            _repositoryMock.Setup(r => r.RelatedTermRepository.Create(It.IsAny<RelatedTermEntity>()))
                .Returns(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1); 

            _mapperMock.Setup(m => m.Map<RelatedTermDto>(entity))
                .Returns(relatedTermsDTO);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(relatedTermsDTO);
        }


        [Fact]
        public async Task Handle_ReturnsError_WhenRelatedTermIsNull()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new CreateRelatedTermCommand(relatedTermsDTO);
            var relatedTerms = new List<RelatedTermEntity>(); 
            var entity = new RelatedTermEntity();


            _mapperMock.Setup(m => m.Map<RelatedTermEntity>(query.RelatedTerm)) 
                .Returns((RelatedTermEntity)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot create new related word for a term!");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot create new related word for a term!"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenExistingTermsAreNull()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new CreateRelatedTermCommand(relatedTermsDTO);
            var relatedTerms = new List<RelatedTermEntity>(); 
            var entity = new RelatedTermEntity();


            _mapperMock.Setup(m => m.Map<RelatedTermEntity>(query.RelatedTerm)).Returns(entity);

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
                .ReturnsAsync((IEnumerable<RelatedTermEntity>)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Слово з цим визначенням уже існує");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Слово з цим визначенням уже існує"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSavingFailed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new CreateRelatedTermCommand(relatedTermsDTO);
            var relatedTerms = new List<RelatedTermEntity>(); // Simulate no existing related terms
            var entity = new RelatedTermEntity();


            _mapperMock.Setup(m => m.Map<RelatedTermEntity>(query.RelatedTerm)) 
                            .Returns(entity);

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
                .ReturnsAsync(relatedTerms);

            _repositoryMock.Setup(r => r.RelatedTermRepository.Create(It.IsAny<RelatedTermEntity>()))
                .Returns(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(0); 

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot save changes in the database after related word creation!");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot save changes in the database after related word creation!"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenMappingToDtoFailed()
        {
            // Arrange
            var relatedTermsDTO = new RelatedTermDto();
            var query = new CreateRelatedTermCommand(relatedTermsDTO);
            var relatedTerms = new List<RelatedTermEntity>(); 
            var entity = new RelatedTermEntity();


            _mapperMock.Setup(m => m.Map<RelatedTermEntity>(query.RelatedTerm))
                            .Returns(entity);

            _repositoryMock.Setup(r => r.RelatedTermRepository.GetAllAsync(
                    It.IsAny<Expression<Func<RelatedTermEntity, bool>>>(), null))
                .ReturnsAsync(relatedTerms);

            _repositoryMock.Setup(r => r.RelatedTermRepository.Create(It.IsAny<RelatedTermEntity>()))
                .Returns(entity);

            _repositoryMock.Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<RelatedTermDto>(entity))
                .Returns((RelatedTermDto)null);


            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().Contain(error => error.Message == "Cannot map entity!");
            _loggerMock.Verify(l => l.LogError(It.IsAny<object>(), "Cannot map entity!"), Times.Once);
        }
    }
}
