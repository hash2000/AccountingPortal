using AccountingPortal.Options;
using LibProfiles.Models;
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
        // тестовые данные вместо использования базы данных
        private List<Profile> profiles = new()
        {
            new Profile { Login = "admin", Password = "admin", Role = "admin" },
            new Profile { Login = "qwerty", Password = "55555", Role = "user" }
        };

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            var profile = profiles.FirstOrDefault(n =>
                n.Login == username &&
                n.Password == password);

            if (profile == null)
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, profile.Login),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, profile.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }

        [HttpPost("/security/admin/auth")]
        public IActionResult Token(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                return BadRequest(new
                {
                    ErrorText = "Invalid username or password."
                });
            }

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
                );

            var jwtEncoded = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            return Json(new
            {
                access_token = jwtEncoded,
                username = identity.Name
            });
        }

        [HttpPost("/security/admin/logoff")]
        public void Logoff()
        {
            
        }
    }
}
