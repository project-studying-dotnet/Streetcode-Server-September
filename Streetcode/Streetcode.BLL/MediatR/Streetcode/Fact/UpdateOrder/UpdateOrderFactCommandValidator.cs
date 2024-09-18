using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.UpdateOrder
{
    public class UpdateOrderFactCommandValidator : AbstractValidator<UpdateOrderFactCommand>
    {
        public UpdateOrderFactCommandValidator(IRepositoryWrapper repositoryWrapper)
        {
            RuleFor(x => x.Fact.FactId)
                .Must((factId, _) =>
                {
                    return repositoryWrapper.FactRepository
                        .GetFirstOrDefaultAsync(fact => fact.Id == factId.Fact.FactId)
                        .Result != null;
                })
                .WithMessage("Fact Id doesn't exist");

            RuleFor(x => x.Fact.StreetcodeId)
                .Must((StreetcodeId, _) =>
                {
                    return repositoryWrapper.StreetcodeRepository
                        .GetFirstOrDefaultAsync(Streetcode => Streetcode.Id == StreetcodeId.Fact.StreetcodeId)
                        .Result != null;
                })
                .WithMessage("Streetcode Id doesn't exist");

            RuleFor(x => x.Fact.NewOrder)
                .Must((SortOrder, _) =>
                {
                    return repositoryWrapper.FactRepository
                        .GetFirstOrDefaultAsync(fact => fact.SortOrder == SortOrder.Fact.NewOrder)
                        .Result != null;
                })
                .WithMessage("SortOrder position is wrong");
        }
    }
}
