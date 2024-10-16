using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Transactions;

namespace Streetcode.BLL.MediatR.Transactions.TransactionLink.GetAll;

public record GetAllTransactLinksQuery : IRequest<Result<IEnumerable<TransactLinkDto>>>;