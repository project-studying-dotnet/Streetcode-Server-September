﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streetcode.BLL.Dto.Users;

public class TokenResultDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}