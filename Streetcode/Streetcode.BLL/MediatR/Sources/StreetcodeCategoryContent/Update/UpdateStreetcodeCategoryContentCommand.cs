using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Streetcode.BLL.Dto.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Update
{
    public record UpdateStreetcodeCategoryContentCommand(int streetcodeId, int categoryId, 
                                                        SourceLinkCategoryContentUpdateDto CategoryContentUpdateDto)
    : IRequest<Result<SourceLinkCategoryContentUpdateDto>>;
}
