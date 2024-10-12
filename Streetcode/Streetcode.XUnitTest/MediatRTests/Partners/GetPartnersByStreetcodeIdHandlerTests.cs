using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetByStreetcodeId;
using Streetcode.DAL.Entities.Partners;
using Streetcode.DAL.Entities.Streetcode;
using Streetcode.DAL.Enums;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Streetcode.XUnitTest.MediatRTests.Partners
{
    public class GetPartnersByStreetcodeIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetPartnersByStreetcodeIdHandler _handler;

        public GetPartnersByStreetcodeIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetPartnersByStreetcodeIdHandler(_mapperMock.Object, _repositoryWrapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnersAreFound()
        {
            // Arrange
            var testStreetcodeId = 1;
            var testStreetcode = new StreetcodeContent
            {
                Id = testStreetcodeId,
            };

            var testPartners = new List<Partner>
            {
                new Partner
                {
                    Id = 1,
                    Title = "Partner 1",
                    IsVisibleEverywhere = false,
                    Streetcodes = new List<StreetcodeContent> { testStreetcode },
                    PartnerSourceLinks = new List<PartnerSourceLink>
                    {
                        new PartnerSourceLink
                        {
                            Id = 1,
                            LogoType = LogoType.Facebook,
                            TargetUrl = "http://facebook.com/partner1"
                        }
                    }
                },
                new Partner
                {
                    Id = 2,
                    Title = "Partner 2",
                    IsVisibleEverywhere = true,
                    PartnerSourceLinks = new List<PartnerSourceLink>
                    {
                        new PartnerSourceLink
                        {
                            Id = 2,
                            LogoType = LogoType.Twitter,
                            TargetUrl = "http://twitter.com/partner2"
                        }
                    }
                }
            };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.Is<Expression<Func<StreetcodeContent, bool>>>(predicate =>
                    predicate.Compile().Invoke(testStreetcode)
                ),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            )).ReturnsAsync(testStreetcode);

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            )).ReturnsAsync(testPartners);

            var testPartnerDtos = new List<PartnerDto>
            {
                new PartnerDto
                {
                    Id = 1,
                    Title = "Partner 1",
                    IsVisibleEverywhere = false,
                    PartnerSourceLinks = new List<PartnerSourceLinkDto>
                    {
                        new PartnerSourceLinkDto
                        {
                            Id = 1,
                            LogoType = new LogoTypeDto { },
                            TargetUrl = new UrlDto { Href = "http://facebook.com/partner1" }
                        }
                    }
                },
                new PartnerDto
                {
                    Id = 2,
                    Title = "Partner 2",
                    IsVisibleEverywhere = true,
                    PartnerSourceLinks = new List<PartnerSourceLinkDto>
                    {
                        new PartnerSourceLinkDto
                        {
                            Id = 2,
                            LogoType = new LogoTypeDto { },
                            TargetUrl = new UrlDto { Href = "http://twitter.com/partner2" }
                        }
                    }
                }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<PartnerDto>>(It.Is<IEnumerable<Partner>>(partners =>
                partners.SequenceEqual(testPartners)
            ))).Returns(testPartnerDtos);

            // Act
            var result = await _handler.Handle(new GetPartnersByStreetcodeIdQuery(testStreetcodeId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testPartnerDtos, options =>
                options.ComparingByMembers<PartnerDto>()
                       .WithStrictOrdering());

            _repositoryWrapperMock.Verify(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            ), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            ), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<PartnerDto>>(It.Is<IEnumerable<Partner>>(partners =>
                partners.SequenceEqual(testPartners)
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenStreetcodeNotFound()
        {
            // Arrange
            var testStreetcodeId = 1;

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            )).ReturnsAsync((StreetcodeContent)null);

            var query = new GetPartnersByStreetcodeIdQuery(testStreetcodeId);
            string errorMsg = $"Cannot find any partners with corresponding streetcode id: {testStreetcodeId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(errorMsg);

            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            ), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            ), Times.Never);
        }


        [Fact]
        public async Task Handle_ReturnsFailResult_WhenNoPartnersFound()
        {
            // Arrange
            var testStreetcodeId = 1;
            var testStreetcode = new StreetcodeContent
            {
                Id = testStreetcodeId,
            };

            _repositoryWrapperMock.Setup(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.Is<Expression<Func<StreetcodeContent, bool>>>(predicate =>
                    predicate.Compile().Invoke(testStreetcode)
                ),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            )).ReturnsAsync(testStreetcode);

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            )).ReturnsAsync((IEnumerable<Partner>)null);

            var query = new GetPartnersByStreetcodeIdQuery(testStreetcodeId);
            string errorMsg = $"Cannot find a partners by a streetcode id: {testStreetcodeId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(errorMsg);

            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.StreetcodeRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<StreetcodeContent, bool>>>(),
                It.IsAny<Func<IQueryable<StreetcodeContent>, IIncludableQueryable<StreetcodeContent, object>>>()
            ), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            ), Times.Once);
        }
    }
}
