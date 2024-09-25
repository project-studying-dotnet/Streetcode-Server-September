using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.AdditionalContent;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetById;
using Streetcode.DAL.Entities.Partners;
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
    public class GetPartnerByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetPartnerByIdHandler _handler;
        private readonly IMapper _mapper;

        public GetPartnerByIdHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _loggerMock = new Mock<ILoggerService>();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Partner, PartnerDto>();
                cfg.CreateMap<PartnerSourceLink, PartnerSourceLinkDto>();
                cfg.CreateMap<LogoType, LogoTypeDto>();
                cfg.CreateMap<string, UrlDto>()
                    .ForMember(dest => dest.Href, opt => opt.MapFrom(src => src));
            });

            _mapper = configuration.CreateMapper();
            _handler = new GetPartnerByIdHandler(_repositoryWrapperMock.Object, _mapper, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnerIsFound()
        {
            // Arrange
            var testPartnerId = 1;
            var testPartner = new Partner
            {
                Id = testPartnerId,
                IsKeyPartner = true,
                IsVisibleEverywhere = false,
                Title = "Test Partner",
                Description = "Test Description",
                LogoId = 123,
                TargetUrl = "http://partner-url.com",
                PartnerSourceLinks = new List<PartnerSourceLink>
                {
                    new PartnerSourceLink
                    {
                        Id = 1,
                        LogoType = LogoType.Facebook,
                        TargetUrl = "http://facebook.com/testpartner"
                    }
                },
            };

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetSingleOrDefaultAsync(
                It.Is<Expression<Func<Partner, bool>>>(predicate =>
                    predicate.Compile()(testPartner)
                ),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            )).ReturnsAsync(testPartner);

            // Act
            var result = await _handler.Handle(new GetPartnerByIdQuery(testPartnerId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();

            var expectedDto = _mapper.Map<PartnerDto>(testPartner);

            result.Value.Should().BeEquivalentTo(expectedDto, options =>
                options.ComparingByMembers<PartnerDto>()
                       .ComparingByMembers<PartnerSourceLinkDto>()
                       .ComparingByMembers<UrlDto>()
                       .ComparingByMembers<LogoTypeDto>()
            );

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetSingleOrDefaultAsync(
                It.Is<Expression<Func<Partner, bool>>>(predicate =>
                    predicate.Compile()(testPartner)
                ),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            ), Times.Once);
        }


        [Fact]
        public async Task Handle_ReturnsFailResult_WhenPartnerIsNotFound()
        {
            // Arrange
            var testPartnerId = 1;
            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            )).ReturnsAsync((Partner)null);

            var query = new GetPartnerByIdQuery(testPartnerId);
            string errorMsg = $"Cannot find any partner with corresponding id: {testPartnerId}";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(errorMsg);
            _loggerMock.Verify(logger => logger.LogError(query, errorMsg), Times.Once);

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetSingleOrDefaultAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            ), Times.Once);
        }

    }
}
