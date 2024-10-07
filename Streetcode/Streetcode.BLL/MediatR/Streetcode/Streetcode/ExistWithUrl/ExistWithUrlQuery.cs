using FluentResults;
using MediatR;

namespace Streetcode.BLL.MediatR.Streetcode.Streetcode.ExistWithUrl;

public record ExistWithUrlQuery(string TransliterationUrl) : IRequest<Result<bool>>;