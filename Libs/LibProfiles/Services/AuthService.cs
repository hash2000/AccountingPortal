using LibProfiles.Context;
using LibProfiles.Context.Results;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LibProfiles.Services
{
    public class AuthService
    {
        public ProfilesContext Context { get; set; }

        // издатель токена
        public const string ISSUER = "MyAuthServer";

        // потребитель токена
        public const string AUDIENCE = "MyAuthClient";

        // ключ для шифрации
        public const string KEY = "mysupersecret_secretkey!123";

        // время жизни токена - 1 минута
        public const int LIFETIME = 1;

        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
        }


        private ClaimsIdentity GetIdentity(string username, string password)
        {
            var profile = Context.Profiles.FirstOrDefault(n =>
                n.Login == username &&
                n.Password == password);

            if (profile == null)
                return null;

            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, profile.Login),
               // new Claim(ClaimsIdentity.DefaultRoleClaimType, profile.Role)
            };

            var claimsIdentity = new ClaimsIdentity(claims, "Token",
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            return claimsIdentity;
        }

        public JwtIdentityResult GetToken(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
                return null;

            var now = DateTime.UtcNow;

            var jwt = new JwtSecurityToken(
                issuer: ISSUER,
                audience: AUDIENCE,
                notBefore: now,
                claims: identity.Claims,
                expires: now.Add(TimeSpan.FromMinutes(LIFETIME)),
                signingCredentials: new SigningCredentials(
                    GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256)
                );

            var jwtEncoded = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            return new JwtIdentityResult
            {
                access_token = jwtEncoded,
                username = identity.Name
            };
        }
    }
}
