using AutoMapper;
using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Http;
using Streetcode.BLL.Dto.Streetcode.TextContent.Fact;
using Streetcode.BLL.Exceptions.CustomExceptions;
using Streetcode.DAL.Repositories.Interfaces.Base;

using factEntety = Streetcode.DAL.Entities.Streetcode.TextContent.Fact;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Create
{
    public class CreateFactHandler : IRequestHandler<CreateFactCommand, Result<FactDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repository;

        public CreateFactHandler(IMapper mapper, IRepositoryWrapper repository)
        {
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<Result<FactDto>> Handle(CreateFactCommand request, CancellationToken cancellationToken)
        {     
            var newFact = _mapper.Map<factEntety>(request.Fact);

            if (newFact is null)
                throw new CustomException("Cannot convert null to fact", StatusCodes.Status204NoContent);

            var facts = await _repository.FactRepository
                    .GetAllAsync(f => f.StreetcodeId == newFact.StreetcodeId);

            var maxOrderInFacts = facts.Select(f => f.SortOrder).Max();

            newFact.SortOrder = maxOrderInFacts + 1;

            var entity = await _repository.FactRepository.CreateAsync(newFact);
            var resultIsSuccess = await _repository.SaveChangesAsync() > 0;

            if (resultIsSuccess)
            {
                return Result.Ok(_mapper.Map<FactDto>(entity));
            }
            else
            {
                throw new CustomException("Failed to create a fact", StatusCodes.Status400BadRequest);
            }
        }
    }
}
