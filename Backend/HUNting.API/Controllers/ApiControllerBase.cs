using System.Security.Claims;
using HUNting.API.Models.Users;
using Microsoft.AspNetCore.Mvc;

namespace HUNting.API.Controllers
{
    /// <summary>
    /// Közös ős a controllerekhez: a bejelentkezett felhasználóval kapcsolatos,
    /// korábban controllerenként duplikált segédfüggvények.
    /// </summary>
    public abstract class ApiControllerBase : ControllerBase
    {
        protected int? GetUserId()
        {
            var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(idClaim, out var id) ? id : null;
        }

        protected bool IsAdministrator => User.IsInRole("Administrator");

        protected bool IsSocietyAccount()
        {
            var accountType = User.FindFirstValue("account_type");
            return Enum.TryParse<AccountType>(accountType, true, out var parsed) && parsed == AccountType.Society;
        }
    }
}
