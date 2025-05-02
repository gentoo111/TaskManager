// AuthController.cs
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManager.Core.DTOs;
using TaskManager.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.Data;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(IAuthService authService, ILogger<AuthController> logger,
            ApplicationDbContext context)
        {
            _authService = authService;
            _logger = logger;
            _context = context;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Register(RegisterDto registerDto)
        {
            try
            {
                _logger.LogInformation("Registration request received for user: {Username}, email: {Email}", 
                    registerDto.Username, registerDto.Email);
                
                var result = await _authService.RegisterAsync(registerDto);
                
                if (result.Successful)
                {
                    _logger.LogInformation("Registration successful for user: {Username}", registerDto.Username);
                    return Ok(result);
                }
                
                _logger.LogWarning("Registration failed for user: {Username}, reason: {Message}", 
                    registerDto.Username, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", registerDto.Username);
                return StatusCode(500, new AuthResponse { 
                    Successful = false, 
                    Message = "Registration failed due to an error" 
                });
            }
        }

		[HttpPost("syncUser")]
		[AllowAnonymous]
		public async Task<ActionResult<AuthResponse>> SyncUser([FromBody] SyncUserDto syncUserDto)
		{
   		 try
   		 {
       		 _logger.LogInformation("User sync request received for userId: {UserId}, email: {Email}, username: {Username}", 
         		   syncUserDto.UserId, syncUserDto.Email, syncUserDto.Username);
        
       		 // Sync user information with local database
       		 await _authService.SyncLocalUserAsync(
        		    syncUserDto.UserId, 
       		     syncUserDto.Email,
      		      syncUserDto.Username
      		  );
        
        	return Ok(new AuthResponse { 
       		     Successful = true, 
        		    Message = "User synchronized with local database." 
        		});
    		}
    		catch (Exception ex)
   		 {
      		  _logger.LogError(ex, "Error during user sync for: {Username}", syncUserDto.Username);
     		   return StatusCode(500, new AuthResponse { 
    		        Successful = false, 
   		         Message = "User sync failed due to an error" 
  		      });
   		 }
		}

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponse>> Login(LoginDto loginDto)
        {
            try
            {
                _logger.LogInformation("Login attempt for email: {Email}", loginDto.Email);
                
                var result = await _authService.LoginAsync(loginDto);
                
                if (result.Successful)
                {
                    _logger.LogInformation("Login successful for email: {Email}", loginDto.Email);
                    return Ok(result);
                }
                
                _logger.LogWarning("Login failed for email: {Email}, reason: {Message}", 
                    loginDto.Email, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                return StatusCode(500, new AuthResponse { 
                    Successful = false, 
                    Message = "Login failed due to an error" 
                });
            }
        }

     

		
    }
}