using AutoMapper;
using Moq;
using Streetcode.BLL.Dto.Partners.Create;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.Update;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore.Query;
using FluentAssertions;

namespace Streetcode.XUnitTest.MediatRTests.Partners
{
    public class UpdatePartnerHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly UpdatePartnerHandler _handler;

        public UpdatePartnerHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new UpdatePartnerHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnerIsUpdatedSuccessfully()
        {
            // Arrange
            var createPartnerDto = new CreatePartnerDto
            {
                Id = 1,
                Title = "Updated Partner",
                PartnerSourceLinks = new List<CreatePartnerSourceLinkDto>
                {
                    new CreatePartnerSourceLinkDto { Id = 1, TargetUrl = "http://newlink1.com" },
                    new CreatePartnerSourceLinkDto { Id = 2, TargetUrl = "http://newlink2.com" }
                },
                Streetcodes = new List<StreetcodeShortDto>
                {
                    new StreetcodeShortDto { Id = 1 },
                    new StreetcodeShortDto { Id = 2 }
                }
            };

            var partnerEntity = new Partner
            {
                Id = createPartnerDto.Id,
                Title = createPartnerDto.Title,
                PartnerSourceLinks = createPartnerDto.PartnerSourceLinks.Select(linkDto => new PartnerSourceLink
                {
                    Id = linkDto.Id,
                    TargetUrl = linkDto.TargetUrl
                }).ToList(),
                Streetcodes = new List<StreetcodeContent>()
            };

            _mapperMock.Setup(m => m.Map<Partner>(It.IsAny<CreatePartnerDto>()))
                .Returns(partnerEntity);

            var existingLinks = new List<PartnerSourceLink>
            {
                new PartnerSourceLink { Id = 1, PartnerId = createPartnerDto.Id, TargetUrl = "http://oldlink1.com" },
                new PartnerSourceLink { Id = 3, PartnerId = createPartnerDto.Id, TargetUrl = "http://oldlink3.com" }
            };

            _repositoryWrapperMock.Setup(repo => repo.PartnerSourceLinkRepository.GetAllAsync(
                It.IsAny<Expression<Func<PartnerSourceLink, bool>>>(),
                It.IsAny<Func<IQueryable<PartnerSourceLink>, IIncludableQueryable<PartnerSourceLink, object>>>()
            )).ReturnsAsync(existingLinks);

            var existingPartnerStreetcodes = new List<StreetcodePartner>
            {
                new StreetcodePartner { PartnerId = createPartnerDto.Id, StreetcodeId = 1 },
                new StreetcodePartner { PartnerId = createPartnerDto.Id, StreetcodeId = 3 }
            };

            _repositoryWrapperMock.Setup(repo => repo.PartnerStreetcodeRepository.GetAllAsync(
                It.IsAny<Expression<Func<StreetcodePartner, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodePartner>, IIncludableQueryable<StreetcodePartner, object>>>()
            )).ReturnsAsync(existingPartnerStreetcodes);

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.Update(It.IsAny<Partner>()));
            _repositoryWrapperMock.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(1);

            _mapperMock.Setup(m => m.Map<PartnerDto>(It.IsAny<Partner>()))
                .Returns(new PartnerDto
                {
                    Id = createPartnerDto.Id,
                    Title = createPartnerDto.Title,
                    Streetcodes = createPartnerDto.Streetcodes
                });

            // Act
            var result = await _handler.Handle(new UpdatePartnerQuery(createPartnerDto), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();
            result.Value.Id.Should().Be(createPartnerDto.Id);
            result.Value.Title.Should().Be(createPartnerDto.Title);
            result.Value.Streetcodes.Should().BeEquivalentTo(createPartnerDto.Streetcodes);


            _repositoryWrapperMock.Verify(repo => repo.PartnerSourceLinkRepository.Delete(It.Is<PartnerSourceLink>(l => l.Id == 3)), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.Update(It.IsAny<Partner>()), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.SaveChangesAsync(), Times.Exactly(2));
            _repositoryWrapperMock.Verify(repo => repo.PartnerStreetcodeRepository.Delete(It.Is<StreetcodePartner>(ps => ps.StreetcodeId == 3)), Times.Once);
            _repositoryWrapperMock.Verify(repo => repo.PartnerStreetcodeRepository.CreateAsync(It.Is<StreetcodePartner>(ps => ps.StreetcodeId == 2)), Times.Once);

            _mapperMock.Verify(m => m.Map<PartnerDto>(It.IsAny<Partner>()), Times.Once);
        }


        [Fact]
        public async Task Handle_ReturnsFailResult_WhenExceptionIsThrown()
        {
            // Arrange
            var createPartnerDto = new CreatePartnerDto
            {
                Id = 1,
                Title = "Updated Partner",
            };

            var exceptionMessage = "Test exception";

            _mapperMock.Setup(m => m.Map<Partner>(It.IsAny<CreatePartnerDto>()))
                .Throws(new Exception(exceptionMessage));

            // Act
            var result = await _handler.Handle(new UpdatePartnerQuery(createPartnerDto), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.Should().ContainSingle();
            result.Errors[0].Message.Should().Be(exceptionMessage);

            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), exceptionMessage), Times.Once);
        }
    }
}
