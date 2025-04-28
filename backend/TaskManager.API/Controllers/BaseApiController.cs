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
            _logger.LogInformation("所有声明: {@Claims}", claims.Select(c => new { c.Type, c.Value }));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("通过 NameIdentifier 找到的 UserId: {UserId}", userId);

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value;
                _logger.LogInformation("通过 nameid 找到的 UserId: {UserId}", userId);
            }

            if (string.IsNullOrEmpty(userId))
            {
                userId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
                _logger.LogInformation("通过 sub 找到的 UserId: {UserId}", userId);
            }

            return userId;
        }
    }
}