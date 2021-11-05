using LibProfiles.Context.Models;
using LibProfiles.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AccountingPortal.Controllers
{
    public class AuthController : Controller
    {

        public AuthService AuthService { get; set; }


        [HttpPost("/security/admin/auth")]
        public IActionResult Token(string username, string password)
        {
            return Json(AuthService.GetToken(username, password));
        }

        [HttpPost("/security/admin/logoff")]
        public void Logoff()
        {

        }
    }
}
