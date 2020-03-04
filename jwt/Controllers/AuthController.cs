using System;
using System.IdentityModel.Tokens.Jwt;
// using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using jwt.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace jwt.Controllers
{
    public class AuthController : Controller
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager
            )
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // username: tandi@live.com password: Password!1            
            var user = await _userManager.FindByNameAsync(model.Username);     
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                await _signInManager.SignInAsync(user, null);
                var claims = new[] {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                // key must be at least 16 chars for SHA256
                var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("MySuperSecretKey"));

                var token = new JwtSecurityToken(
                    issuer: "http://halo.world.com",
                    audience: "http://halo.world.com",
                    expires: DateTime.UtcNow.AddHours(1),
                    claims: claims,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );

                return View("Tokens", (
                    new JwtSecurityTokenHandler().WriteToken(token),
                    token.ValidTo
                ));
            }

            return Unauthorized();
        }
    }
}