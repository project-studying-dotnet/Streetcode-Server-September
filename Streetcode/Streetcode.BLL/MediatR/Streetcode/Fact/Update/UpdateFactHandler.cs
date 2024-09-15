using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.News;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.BlobStorage;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.BLL.MediatR.Newss.Update;
using Streetcode.DAL.Entities.News;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FactEntety =  Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactHandler : IRequestHandler<UpdateFactCommand, Result<FactUpdateDto>>
    {
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly IMapper _mapper;
        private readonly ILoggerService _logger;
        public UpdateFactHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<FactUpdateDto>> Handle(UpdateFactCommand request, CancellationToken cancellationToken)
        {
            var fact = _mapper.Map<FactEntety>(request.Fact);

            if (fact is null)
            {
                const string errorMsg = $"Cannot convert null to fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }

            var response = _mapper.Map<FactUpdateDto>(fact);

            _repositoryWrapper.FactRepository.Update(fact);
            bool resultIsSuccess = await _repositoryWrapper.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(response);
            }
            else
            {
                const string errorMsg = $"Failed to update fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
