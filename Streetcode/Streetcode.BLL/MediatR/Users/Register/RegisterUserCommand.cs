using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Users.Register
{
    public record RegisterUserCommand(RegisterUserDto UserDto) 
        : IRequest<Result<UserDto>>;
}
