using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Users
{
    public class RefreshTokenResponceDto
    {
        public string Token { get; set; } = null!;
        public DateTime ExpireAt { get; set; }
    }
}
