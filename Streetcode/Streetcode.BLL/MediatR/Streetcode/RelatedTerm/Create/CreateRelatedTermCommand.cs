﻿using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Streetcode.TextContent;

namespace Streetcode.BLL.MediatR.Streetcode.RelatedTerm.Create
{
    public record CreateRelatedTermCommand(RelatedTermDto RelatedTerm) : IRequest<Result<RelatedTermDto>>
    {
    }
}
