using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode;
using Streetcode.BLL.MediatR.Media.Audio.Create;
using Streetcode.BLL.MediatR.Media.Image.Create;
using System.Transactions;
using AutoMapper;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.Create;

public class CreateStreetcodeCoordinator : IRequestHandler<CreateStreetcodeMainBlockCommand, Result<int>>
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    public CreateStreetcodeCoordinator(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    public async Task<Result<int>> Handle(CreateStreetcodeMainBlockCommand request, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        var streetcodeMainBlock = request.StreetcodeMainBlockCreateDto;
        var streetcodeCreateDto = _mapper.Map<StreetcodeCreateDto>(streetcodeMainBlock);
        
        var createBlackAndWhiteImageResult = await _mediator.Send(
            new CreateImageCommand(streetcodeMainBlock.BlackAndWhiteImageFileBaseCreateDto), 
            cancellationToken);
        streetcodeCreateDto.BlackAndWhiteImageDto = createBlackAndWhiteImageResult.Value;
        
        if (streetcodeMainBlock.AudioFileBaseCreate is not null)
        {
            var createAudioResult = await _mediator.Send(
                new CreateAudioCommand(streetcodeMainBlock.AudioFileBaseCreate), 
                cancellationToken);
            streetcodeCreateDto.AudioDto = createAudioResult.Value;
        }
        
        if (streetcodeMainBlock.HistoryLinksImageFileBaseCreateDto is not null)
        {
            var createHistoryLinksImageResult = await _mediator.Send(
                new CreateImageCommand(streetcodeMainBlock.HistoryLinksImageFileBaseCreateDto), 
                cancellationToken);
            streetcodeCreateDto.HistoryLinksImageDto = createHistoryLinksImageResult.Value;
        }

        if (streetcodeMainBlock.GifFileBaseCreateDto is not null)
        {
            var createGifImageResult = await _mediator.Send(
                new CreateImageCommand(streetcodeMainBlock.GifFileBaseCreateDto), 
                cancellationToken);
            streetcodeCreateDto.GifDto = createGifImageResult.Value;
        }
        
        var createStreetcodeResult = await _mediator.Send(
            new CreateStreetcodeCommand(streetcodeCreateDto), 
            cancellationToken);

        transactionScope.Complete();
        
        return Result.Ok(createStreetcodeResult.Value);
    }
}