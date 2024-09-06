using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Transactions;
using Streetcode.BLL.MediatR.ResultVariations;

namespace Streetcode.BLL.MediatR.Transactions.TransactionLink.GetByStreetcodeId;

public record GetTransactLinkByStreetcodeIdQuery(int StreetcodeId) : IRequest<Result<TransactLinkDto?>>;