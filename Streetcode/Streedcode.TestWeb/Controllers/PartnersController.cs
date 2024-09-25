using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Streedcode.TestWeb.Models;

namespace Streedcode.TestWeb.Controllers
{
    public class PartnersController : Controller
    {
        public IActionResult Index()
        {
            var partnrList = new List<PartnerDto>()
            {
                new PartnerDto
                {
                    Id = 1,
                    IsKeyPartner = true,
                    IsVisibleEverywhere = true,
                    Title = "Партнер 1",
                    Description = "Опис для партнера 1",
                    LogoId = 101
                },
                new PartnerDto
                {
                    Id = 2,
                    IsKeyPartner = false,
                    IsVisibleEverywhere = true,
                    Title = "Партнер 2",
                    Description = "Опис для партнера 2",
                    LogoId = 102
                },
                new PartnerDto
                {
                    Id = 3,
                    IsKeyPartner = true,
                    IsVisibleEverywhere = false,
                    Title = "Партнер 3",
                    Description = "Опис для партнера 3",
                    LogoId = 103
                }
            };
            return View(partnrList);
        }
    }
}
