using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using ExemploJWT.Database;
using ExemploJWT.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ExemploJWT.Controllers
{

    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly UserDAO userDAO;

        public AuthController(UserDAO userDAO)
        {
            this.userDAO = userDAO;
        }

        [HttpPost("[action]")]
        [AllowAnonymous]
        public object Login(
            [FromBody] User user,
            [FromServices] SigningConfigurations signingConfigurations,
            [FromServices] TokenConfigurations tokenConfigurations)
        {
            bool credenciaisValidas = false;
            if (user != null && !string.IsNullOrWhiteSpace(user.UserId))
            {
                var usuarioEncontrado = userDAO.Find(user.UserId);

                credenciaisValidas = (usuarioEncontrado != null &&
                    usuarioEncontrado.UserId.Equals(user.UserId) &&
                    usuarioEncontrado.AccessKey.Equals(user.AccessKey));
            }

            if (!credenciaisValidas)
            {
                return new
                {
                    authenticated = false,
                    message = "Falha ao autenticar"
                };
            }
            else
            {
                ClaimsIdentity identity = new ClaimsIdentity(
                    new GenericIdentity(user.UserId, "Login"),
                    new[] {
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
                        new Claim(JwtRegisteredClaimNames.UniqueName, user.UserId),
                        new Claim("Id", "1")
                    }
                );

                DateTime dataCriacao = DateTime.Now;
                DateTime dataExpiracao = dataCriacao +
                    TimeSpan.FromSeconds(tokenConfigurations.Seconds);

                var handler = new JwtSecurityTokenHandler();
                var securityToken = handler.CreateToken(new SecurityTokenDescriptor
                {
                    Issuer = tokenConfigurations.Issuer,
                    Audience = tokenConfigurations.Audience,
                    SigningCredentials = signingConfigurations.SigningCredentials,
                    Subject = identity,
                    NotBefore = dataCriacao,
                    Expires = dataExpiracao
                });
                var token = handler.WriteToken(securityToken);

                return new
                {
                    authenticated = true,
                    message = "OK",
                    created = dataCriacao.ToString("yyyy-MM-dd HH:mm:ss"),
                    expiration = dataExpiracao.ToString("yyyy-MM-dd HH:mm:ss"),
                    accessToken = token
                };
            }
        }
    }
}