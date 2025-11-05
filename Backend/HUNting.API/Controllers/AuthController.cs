using System.Linq;
using System.Security.Claims;
using HUNting.API.Models.Users;
using HUNting.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PhoneNumber = request.Phone,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(request.Phone),
                UserName = request.Email
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { message = "A regisztráció sikertelen. Ellenőrizd a megadott adatokat." });
            }

            await _userManager.AddToRoleAsync(user, "Hunter");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new AuthResponse(token.AccessToken, token.ExpiresAt, UserDto.From(user, roles)));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new { message = "Hibás e-mail cím vagy jelszó." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Hibás e-mail cím vagy jelszó." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return Ok(new AuthResponse(token.AccessToken, token.ExpiresAt, UserDto.From(user, roles)));
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            // For JWT logout happens client side. Endpoint exists for symmetry.
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);
            return Ok(UserDto.From(user, roles));
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password, string? Phone);

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);

    public record UserDto(int Id, string Name, string Email, string? Phone, IEnumerable<string> Roles)
    {
        public static UserDto From(ApplicationUser user, IEnumerable<string> roles) =>
            new(user.Id, user.FullName, user.Email ?? string.Empty, user.Phone, roles);
    }
}
