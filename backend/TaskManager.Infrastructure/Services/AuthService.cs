// AuthService.cs
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Core.DTOs;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using TaskManager.Infrastructure.Data;
using BC = BCrypt.Net.BCrypt;

namespace TaskManager.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterDto registerDto)
        {
            // Check if email already exists
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
            {
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Email already registered"
                };
            }

            // Check if username already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Username already taken"
                };
            }

            // Check if passwords match
            if (registerDto.Password != registerDto.ConfirmPassword)
            {
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Passwords do not match"
                };
            }

            // Create user
            var user = new UserEntity
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BC.HashPassword(registerDto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Successful = true,
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Message = "Registration successful"
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginDto loginDto)
        {
            // Find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);
            if (user == null)
            {
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Invalid email or password"
                };
            }

            // Verify password
            if (!BC.Verify(loginDto.Password, user.PasswordHash))
            {
                return new AuthResponse
                {
                    Successful = false,
                    Message = "Invalid email or password"
                };
            }

            // Generate token
            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                Successful = true,
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Message = "Login successful"
            };
        }

        public async Task<UserEntity> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<bool> VerifyUserOwnershipAsync(string userId, int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            return task != null && task.UserId == userId;
        }

        private string GenerateJwtToken(UserEntity user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]);
    
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    // Use the full URI claim type for better compatibility
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}