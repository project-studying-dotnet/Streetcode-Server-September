using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Sources;

namespace Streetcode.BLL.MediatR.Sources.SourceLinkCategory.GetAll
{
    public record GetAllCategoriesQuery : IRequest<Result<IEnumerable<SourceLinkCategoryDto>>>;
}
