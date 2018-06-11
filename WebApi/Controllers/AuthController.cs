﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebApi.Models;
using WebApi.Models.Configuration;
using WebApi.Services;
using WebApi.Services.Interfaces;


namespace WebApi.Controllers
{
    [Produces("application/json")]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private IAuthorizeService _authService;
        private IGenerateSecurityTokens _generateTokensService;
        private ILogger<AuthController> _logger;

        public AuthController(IAuthorizeService authorizeService, IGenerateSecurityTokens generateTokens, ILogger<AuthController> logger)
        {
            _authService = authorizeService;
            _generateTokensService = generateTokens;
            _logger = logger;
        }

        // POST: api/auth
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> PostTokenRequest([FromBody] TokenRequest request)
        {
            try
            {
                // to test NginX, todo remove asap
                _logger.LogCritical("Username {0}, Pass {1}", request.Username, request.Password);
                var user = new User() { Name = request.Username, Password = request.Password };
                //var user = await _authService.AuthorizeWithLoginAndPasswordAsync(request.Username, request.Password);

                if (user != null)
                {
                    var claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Name)
                    };
                    var token = _generateTokensService.GenerateSecurityToken(claims);

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        validTo = token.ValidTo,
                        user = user
                    });
                }

                return NotFound("Błędny login lub hasło");
            }
            catch (Exception ex)
            {
                return BadRequest("Hasło jest nieprawidłowe, skontaktuj się z administratorem");
            }
        }
    }
}
