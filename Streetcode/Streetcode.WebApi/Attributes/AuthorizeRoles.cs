﻿using Microsoft.AspNetCore.Authorization;
using Streetcode.DAL.Enums;
namespace Streetcode.WebApi.Attributes
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params UserRoles[] userRoles)
        {
            Roles = string.Join(",", userRoles.Select(r => r.ToString()).ToArray());
        }
    }
}
