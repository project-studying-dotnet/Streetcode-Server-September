using FluentResults;
using MediatR;
using Streetcode.BLL.Dto.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.MediatR.Users.Logout
{
    public record UserLogoutQuery () : IRequest<Result<Unit>>;
}
