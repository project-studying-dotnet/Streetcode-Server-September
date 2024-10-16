using AutoMapper;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Partners.GetAll;
using Streetcode.DAL.Entities.Partners;
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
    public class GetAllPartnersHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> _repositoryWrapperMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<ILoggerService> _loggerMock;
        private readonly GetAllPartnersHandler _handler;

        public GetAllPartnersHandlerTests()
        {
            _repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            _mapperMock = new Mock<IMapper>();
            _loggerMock = new Mock<ILoggerService>();
            _handler = new GetAllPartnersHandler(_repositoryWrapperMock.Object, _mapperMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Handle_ReturnsOkResult_WhenPartnersAreFound()
        {
            // Arrange
            var testPartners = new List<Partner>
            {
                new Partner { Id = 1, Title = "Partner 1" },
                new Partner { Id = 2, Title = "Partner 2" }
            };

            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()
            )).ReturnsAsync(testPartners);

            var testPartnerDtos = new List<PartnerDto>
            {
                new PartnerDto { Id = 1, Title = "Partner 1" },
                new PartnerDto { Id = 2, Title = "Partner 2" }
            };

            _mapperMock.Setup(m => m.Map<IEnumerable<PartnerDto>>(It.Is<IEnumerable<Partner>>(p =>
            p.Count() == testPartners.Count &&
            p.All(partner => testPartners.Any(tp => tp.Id == partner.Id && tp.Title == partner.Title))))).Returns(testPartnerDtos);

            // Act
            var result = await _handler.Handle(new GetAllPartnersQuery(), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(testPartnerDtos, options =>
            options.ComparingByMembers<PartnerDto>()
            .WithStrictOrdering());

            _repositoryWrapperMock.Verify(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(), 
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()), Times.Once);

            _mapperMock.Verify(m => m.Map<IEnumerable<PartnerDto>>(It.Is<IEnumerable<Partner>>(p =>
                p.Count() == testPartners.Count &&
                p.All(partner => testPartners.Any(tp => tp.Id == partner.Id && tp.Title == partner.Title))
            )), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsFailResult_WhenNoPartnersFound()
        {
            // Arrange
            _repositoryWrapperMock.Setup(repo => repo.PartnersRepository.GetAllAsync(
                It.IsAny<Expression<Func<Partner, bool>>>(),
                It.IsAny<Func<IQueryable<Partner>, IIncludableQueryable<Partner, object>>>()))!.ReturnsAsync((IEnumerable<Partner>)null);

            var query = new GetAllPartnersQuery();
            const string errorMsg = "Cannot find any partners";

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Errors.First().Message.Should().Be(errorMsg);
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<object>(), errorMsg), Times.Once);
        }

    }
}
