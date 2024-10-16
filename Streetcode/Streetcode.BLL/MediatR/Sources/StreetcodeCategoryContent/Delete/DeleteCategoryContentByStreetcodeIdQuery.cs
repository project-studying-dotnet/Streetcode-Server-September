using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Sources.StreetcodeCategoryContent.Delete
{
    public record DeleteCategoryContentByStreetcodeIdQuery(int streetcodeId, int categoryId) : IRequest<Result<Unit>>;
}
