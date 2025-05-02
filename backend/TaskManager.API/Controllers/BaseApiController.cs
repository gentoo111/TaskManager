using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;

namespace TaskManager.API.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        
        private readonly ILogger _logger;

        public BaseApiController(ILogger logger)
        {
            _logger = logger;
        }
        
        protected string GetUserId()
        {
            var claims = User.Claims.ToList();

            // ✅ 强制打印所有 claims
            foreach (var c in claims)
            {
                _logger.LogInformation("Claim - Type: {Type}, Value: {Value}", c.Type, c.Value);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("通过 NameIdentifier 找到的 UserId: {UserId}", userId);

            userId ??= User.FindFirstValue("nameid");
            _logger.LogInformation("通过 nameid 找到的 UserId: {UserId}", userId);

            userId ??= User.FindFirstValue("sub");
            _logger.LogInformation("通过 sub 找到的 UserId: {UserId}", userId);

            return userId;
        }

    }
}