using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Fact.Update;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Streetcode.Fact.Update
{
    public class UpdateFactCommandValidator : AbstractValidator<UpdateFactCommand>
    {
        public UpdateFactCommandValidator(IRepositoryWrapper repositoryWrapper)
        {
            RuleFor(x => x.Fact.Id)
                .Must((factId, _) =>
                {
                    return repositoryWrapper.FactRepository
                        .GetFirstOrDefaultAsync(fact => fact.Id == factId.Fact.Id)
                        .Result != null;
                })
                .WithMessage("Fact Id doesn't exist");

            RuleFor(x => x.Fact.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(68)
                .WithMessage("Title can not exceed 68 characters");

            RuleFor(x => x.Fact.ImageId)
                .Must((imageId, _) =>
                {
                    return repositoryWrapper.ImageRepository
                        .GetFirstOrDefaultAsync(image => image.Id == imageId.Fact.ImageId)
                        .Result != null;
                })
                .WithMessage("Image Id doesn't exist");

            RuleFor(x => x.Fact.FactContent)
                .NotEmpty()
                .WithMessage("Main text is required")
                .MaximumLength(600)
                .WithMessage("Main text can not exceed 600 characters");

            RuleFor(x => x.Fact.StreetcodeId)
            .Must((StreetcodeId, _) =>
            {
                return repositoryWrapper.StreetcodeRepository
                    .GetFirstOrDefaultAsync(Streetcode => Streetcode.Id == StreetcodeId.Fact.StreetcodeId)
                    .Result != null;
            })
            .WithMessage("Streetcode Id doesn't exist");
        }
    }
}
