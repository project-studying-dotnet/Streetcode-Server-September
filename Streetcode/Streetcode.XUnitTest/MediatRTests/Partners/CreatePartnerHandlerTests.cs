using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System.Linq.Expressions;
using Xunit;

using PartnerEntity = Streetcode.DAL.Entities.Partners.Partner;


namespace Streetcode.BLL.MediatR.Partners.Create
{
    public class CreatePartnerHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly CreatePartnerHandler _handler;

        public CreatePartnerHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new CreatePartnerHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnerIsCreatedSuccessfully()
        {
            // Arrange
            var testPartnerDto = new CreatePartnerDto
            {
                Id = 1,
                Title = "Test Partner",
                Streetcodes = new List<StreetcodeShortDto> { new StreetcodeShortDto { Id = 1 } }
            };

            var testPartner = new PartnerEntity { Id = 1, Title = "Test Partner" };
            var testPartnerDtoResult = new PartnerDto { Id = 1, Title = "Test Partner" };

            var command = new CreatePartnerQuery(testPartnerDto);

            _mapperMock.Setup(m => m.Map<PartnerEntity>(testPartnerDto)).Returns(testPartner);
            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.CreateAsync(testPartner)).ReturnsAsync(testPartner);
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);
            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetAllAsync(
            It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
            It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()))
            .ReturnsAsync(new List<StreetcodeContent> { new StreetcodeContent { Id = 1 } });

            _mapperMock.Setup(m => m.Map<PartnerDto>(testPartner)).Returns(testPartnerDtoResult);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testPartnerDtoResult);
            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.CreateAsync(testPartner), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenSaveChangesFails()
        {
            // Arrange
            var testPartnerDto = new CreatePartnerDto
            {
                Id = 1,
                Title = "Test Partner",
                Streetcodes = new List<StreetcodeShortDto> { new StreetcodeShortDto { Id = 1 } }
            };

            var command = new CreatePartnerQuery(testPartnerDto);

            _mapperMock.Setup(m => m.Map<PartnerEntity>(It.IsAny<CreatePartnerDto>()))
                .Returns((PartnerEntity)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().NotBeEmpty();
            result.Errors.First().Message.Should().Contain("Failed to create a partner");

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.CreateAsync(It.IsAny<PartnerEntity>()), Times.Never);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Never);
            _loggerMock.Verify(l => l.LogError(It.IsAny<CreatePartnerQuery>(), It.Is<string>(s => s.Contains("Failed to create a partner"))), Times.Once);
        }
    }
}
