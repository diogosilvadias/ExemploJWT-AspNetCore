using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ExemploJWT.Models;
using ExemploJWT.Database;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExemploJWT.Controllers
{
    [Route("api/[controller]")]
    public class HomeController : Controller
    {
        private readonly UserDAO userDAO;

        public HomeController(UserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[Authorize("Bearer")]
        [Authorize]
        [HttpGet("[action]")]
        public IActionResult About()
        {
            var id = User.FindFirst("Id").Value;

            return Json(new
            {
                id,
                User.Identity.Name
            });
        }
    }
}
