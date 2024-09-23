using FluentValidation;
using Streetcode.BLL.MediatR.Streetcode.Fact.Create;
using Streetcode.DAL.Repositories.Interfaces.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Users.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
           RuleFor(x => x.LoginDto.Email)
               .NotEmpty()
               .WithMessage("Email is required")
               .EmailAddress()
               .WithMessage("Put correct email format");

           RuleFor(x => x.LoginDto.Password)
              .NotEmpty()
              .WithMessage("Password is required")
              .MaximumLength(20)
              .WithMessage("Password can't exceed 20 characters");

        }
    }
}
