using System.Security.Claims;
using HUNting.API.Data;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using HUNting.API.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly HuntingContext _ctx;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            HuntingContext ctx)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _ctx = ctx;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            // Társasági fióknál a társaság neve kötelező – még a felhasználó
            // létrehozása előtt ellenőrizzük, hogy ne maradjon árva fiók.
            if (request.AccountType == AccountType.Society &&
                (request.Society == null || string.IsNullOrWhiteSpace(request.Society.Name)))
            {
                return BadRequest(new { message = "A társaság neve kötelező." });
            }

            var user = new ApplicationUser
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PhoneNumber = request.Phone,
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(request.Phone),
                UserName = request.Email,
                AccountType = request.AccountType
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { message = "A regisztráció sikertelen. Ellenőrizd a megadott adatokat." });
            }

            var defaultRole = request.AccountType == AccountType.Society ? "SocietyManager" : "Hunter";
            await _userManager.AddToRoleAsync(user, defaultRole);

            Society? ownedSociety = null;
            if (request.AccountType == AccountType.Society)
            {
                ownedSociety = new Society
                {
                    Name = request.Society!.Name,
                    Email = request.Society.Email ?? request.Email,
                    Phone = request.Society.Phone ?? request.Phone,
                    Description = request.Society.Description,
                    OwnerUserId = user.Id,
                    PasswordHash = Guid.NewGuid().ToString("N")
                };

                _ctx.Societies.Add(ownedSociety);
                await _ctx.SaveChangesAsync();

                var leaderRole = await _ctx.SocietyRoles.FirstOrDefaultAsync(r => r.Name == "Vezeto");
                if (leaderRole != null)
                {
                    _ctx.SocietyMembers.Add(new SocietyMember
                    {
                        SocietyId = ownedSociety.SocietyId,
                        UserId = user.Id,
                        RoleId = leaderRole.RoleId,
                        JoinedAt = DateTime.UtcNow
                    });
                    await _ctx.SaveChangesAsync();
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            ownedSociety ??= await _ctx.Societies.AsNoTracking().FirstOrDefaultAsync(s => s.OwnerUserId == user.Id);

            return Ok(new AuthResponse(token.AccessToken, token.ExpiresAt, UserDto.From(user, roles, ownedSociety)));
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
            var ownedSociety = await _ctx.Societies.AsNoTracking().FirstOrDefaultAsync(s => s.OwnerUserId == user.Id);

            return Ok(new AuthResponse(token.AccessToken, token.ExpiresAt, UserDto.From(user, roles, ownedSociety)));
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
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
            var ownedSociety = await _ctx.Societies.AsNoTracking().FirstOrDefaultAsync(s => s.OwnerUserId == user.Id);
            return Ok(UserDto.From(user, roles, ownedSociety));
        }
    }

    public record RegisterRequest(string FullName, string Email, string Password, string? Phone, AccountType AccountType = AccountType.Person, SocietyRegistrationRequest? Society = null);

    public record SocietyRegistrationRequest(string Name, string? Email, string? Phone, string? Description);

    public record LoginRequest(string Email, string Password);

    public record AuthResponse(string AccessToken, DateTime ExpiresAt, UserDto User);

    public record SocietySummaryDto(int SocietyId, string Name);

    public record UserDto(int Id, string Name, string Email, string? Phone, IEnumerable<string> Roles, AccountType AccountType, SocietySummaryDto? OwnedSociety)
    {
        public static UserDto From(ApplicationUser user, IEnumerable<string> roles, Society? ownedSociety = null) =>
            new(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.Phone,
                roles,
                user.AccountType,
                ownedSociety is null ? null : new SocietySummaryDto(ownedSociety.SocietyId, ownedSociety.Name));
    }
}
