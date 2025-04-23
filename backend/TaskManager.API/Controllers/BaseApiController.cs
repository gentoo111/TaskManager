using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace TaskManager.API.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected string GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
            }

            return userId;
        }
    }
}