using Microsoft.AspNetCore.Identity;
using Streedcode.Identity.Data;
using Streedcode.Identity.Models;
using Streedcode.Identity.Services.Interfaces;

namespace Streedcode.Identity.Services.Realizations
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AuthService(AppDbContext db, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
    }
}
