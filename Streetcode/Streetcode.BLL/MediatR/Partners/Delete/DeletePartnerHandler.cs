﻿using AutoMapper;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Partners;
using Streetcode.BLL.Interfaces.Logging;
using Streetcode.DAL.Repositories.Interfaces.Base;

namespace Streetcode.BLL.MediatR.Partners.Delete
{
    public class DeletePartnerHandler : IRequestHandler<DeletePartnerQuery, Result<PartnerDto>>
    {
        private readonly IMapper _mapper;
        private readonly IRepositoryWrapper _repositoryWrapper;
        private readonly ILoggerService _logger;

        public DeletePartnerHandler(IRepositoryWrapper repositoryWrapper, IMapper mapper, ILoggerService logger)
        {
            _repositoryWrapper = repositoryWrapper;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<PartnerDto>> Handle(DeletePartnerQuery request, CancellationToken cancellationToken)
        {
            var partner = await _repositoryWrapper.PartnersRepository.GetFirstOrDefaultAsync(p => p.Id == request.id);
            if (partner == null)
            {
                const string errorMsg = "No partner with such id";
                _logger.LogError(request, errorMsg);
                return Result.Fail(errorMsg);
            }
            else
            {
                _repositoryWrapper.PartnersRepository.Delete(partner);
                try
                {
                    await _repositoryWrapper.SaveChangesAsync();
                    return Result.Ok(_mapper.Map<PartnerDto>(partner));
                }
                catch(Exception ex)
                {
                    _logger.LogError(request, ex.Message);
                    return Result.Fail(ex.Message);
                }
            }
        }
    }
}
