using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

using factEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactQuery, Result<FactDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;
        private readonly ILoggerService _logger;

        public CreateFactHandler(IMapper mapper, IRepositoryWrapper repository, ILoggerService logger)
        {
            _mapper = mapper;
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<FactDto>> Handle(CreateFactQuery request, CancellationToken cancellationToken)
        {     
            var newFact = _mapper.Map<factEntety>(request.Fact);

            if (newFact is null)
            {
                const string errorMsg = "Cannot convert null to fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }

            if (newFact.ImageId == 0)
            {
                newFact.ImageId = null;
            }

            var entity = await _repository.FactRepository.CreateAsync(newFact);
            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<FactDto>(entity));
            }
            else
            {
                const string errorMsg = "Failed to create a fact";
                _logger.LogError(request, errorMsg);
                return Result.Fail(new Error(errorMsg));
            }
        }
    }
}
