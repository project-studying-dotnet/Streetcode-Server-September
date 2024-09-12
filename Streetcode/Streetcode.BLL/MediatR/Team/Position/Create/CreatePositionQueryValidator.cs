using FluentValidation;
using Streetcode.BLL.MediatR.Team.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Team.Position.Create
{
    public class CreatePositionQueryValidator: AbstractValidator<CreatePositionQuery>
    {
        public CreatePositionQueryValidator()
        {
            RuleFor(x => x.position.Id)
                .NotEmpty()
                .WithMessage("Id is required.");

            RuleFor(x => x.position.Position)
                .NotEmpty()
                .WithMessage("Position is required.")
                .MaximumLength(50)
                .WithMessage("Position cannot exceed 50 characters.");
        }
    }
}
