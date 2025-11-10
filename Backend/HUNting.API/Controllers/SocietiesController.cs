using System.Security.Claims;
using System.IO;
using HUNting.API.Data;
using HUNting.API.Models.Societies;
using HUNting.API.Models.Users;
using HUNting.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HUNting.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SocietiesController : ControllerBase
    {
        private readonly ISocietyService _svc;
        private readonly HuntingContext _ctx;
        private readonly IWebHostEnvironment _env;

        public SocietiesController(ISocietyService svc, HuntingContext ctx, IWebHostEnvironment env)
        {
            _svc = svc;
            _ctx = ctx;
            _env = env;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Society>>> GetAll() => Ok(await _svc.GetAllAsync());

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Society>> Get(int id)
            => (await _svc.GetByIdAsync(id)) is { } s ? Ok(s) : NotFound();

        [HttpGet("my")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Society>>> GetMySocieties()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var societies = await _svc.GetByUserAsync(userId.Value);
            return Ok(societies);
        }

        [HttpGet("{id:int}/members")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SocietyMember>>> Members(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, userId.Value))
            {
                return Forbid();
            }

            return Ok(await _svc.GetMembersAsync(id));
        }

        [HttpPost("{id:int}/join")]
        [Authorize]
        public async Task<ActionResult<SocietyMembershipRequest>> RequestMembership(int id, MembershipRequestDto request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            if (IsSocietyAccount())
            {
                return BadRequest("Tarsasagi fiok nem csatlakozhat masik tarsasaghoz.");
            }

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            try
            {
                var created = await _svc.CreateMembershipRequestAsync(id, userId.Value, request?.Message);
                return Ok(created);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/requests")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SocietyMembershipRequest>>> GetRequests(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, userId.Value))
            {
                return Forbid();
            }

            var requests = await _svc.GetMembershipRequestsAsync(id);
            return Ok(requests);
        }

        [HttpPost("{id:int}/requests/{userId:int}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveRequest(int id, int userId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, currentUserId.Value))
            {
                return Forbid();
            }

            try
            {
                await _svc.ApproveMembershipRequestAsync(id, userId, currentUserId.Value);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("{id:int}/requests/{userId:int}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectRequest(int id, int userId)
        {
            var currentUserId = GetUserId();
            if (currentUserId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, currentUserId.Value))
            {
                return Forbid();
            }

            try
            {
                await _svc.RejectMembershipRequestAsync(id, userId, currentUserId.Value);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id:int}/posts")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<SocietyPost>>> GetPosts(int id)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isMember = await _ctx.SocietyMembers.AnyAsync(m => m.SocietyId == id && m.UserId == userId.Value);
            var isOwner = await _ctx.Societies.AnyAsync(s => s.SocietyId == id && s.OwnerUserId == userId.Value);

            if (!isMember && !isOwner && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var posts = await _svc.GetPostsAsync(id);
            return Ok(posts);
        }

        [HttpPost("{id:int}/posts")]
        [Authorize]
        [RequestSizeLimit(10_485_760)]
        public async Task<ActionResult<SocietyPost>> CreatePost(int id, [FromForm] SocietyPostCreateRequest request)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, userId.Value))
            {
                return Forbid();
            }

            string? imageUrl = null;
            if (request.Image != null && request.Image.Length > 0)
            {
                try
                {
                    imageUrl = await SaveImageAsync(request.Image);
                }
                catch (ArgumentException ex)
                {
                    return BadRequest(new { message = ex.Message });
                }
            }

            var post = await _svc.AddPostAsync(new SocietyPost
            {
                SocietyId = id,
                AuthorUserId = userId.Value,
                Title = request.Title,
                Content = request.Content,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow
            });

            return CreatedAtAction(nameof(GetPosts), new { id }, post);
        }

        [HttpDelete("{id:int}/posts/{postId:int}")]
        [Authorize]
        public async Task<IActionResult> DeletePost(int id, int postId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var society = await _svc.GetByIdAsync(id);
            if (society == null) return NotFound();

            if (!IsOwnerOrAdmin(society, userId.Value))
            {
                return Forbid();
            }

            var post = await _ctx.SocietyPosts.FirstOrDefaultAsync(p => p.SocietyPostId == postId && p.SocietyId == id);
            if (post == null) return NotFound();

            DeleteImage(post.ImageUrl);
            await _svc.RemovePostAsync(id, postId);
            return NoContent();
        }

        private int? GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out var id) ? id : null;
        }

        private bool IsSocietyAccount()
        {
            var accountType = User.FindFirstValue("account_type");
            return Enum.TryParse<AccountType>(accountType, true, out var parsed) && parsed == AccountType.Society;
        }

        private bool IsOwnerOrAdmin(Society society, int userId)
        {
            if (User.IsInRole("Administrator"))
            {
                return true;
            }

            return society.OwnerUserId == userId;
        }

        private async Task<string> SaveImageAsync(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(extension))
            {
                throw new ArgumentException("Csak JPG, JPEG, PNG vagy WEBP fajl tamogatott.");
            }

            var uploadsRoot = EnsureUploadsDirectory();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var absolutePath = Path.Combine(uploadsRoot, fileName);

            await using var stream = System.IO.File.Create(absolutePath);
            await file.CopyToAsync(stream);

            return "/uploads/societies/" + fileName;
        }

        private string EnsureUploadsDirectory()
        {
            var root = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(root))
            {
                root = Path.Combine(AppContext.BaseDirectory, "wwwroot");
                Directory.CreateDirectory(root);
                _env.WebRootPath = root;
            }

            var societyPath = Path.Combine(root, "uploads", "societies");
            Directory.CreateDirectory(societyPath);
            return societyPath;
        }

        private void DeleteImage(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            var uploads = EnsureUploadsDirectory();
            var fileName = Path.GetFileName(url);
            var absolutePath = Path.Combine(uploads, fileName);
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
    }

    public record MembershipRequestDto(string? Message);

    public record SocietyPostCreateRequest(string? Title, string? Content, IFormFile? Image);
}
