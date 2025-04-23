// IAuthService.cs
using System.Threading.Tasks;
using TaskManager.Core.DTOs;
using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponse> LoginAsync(LoginDto loginDto);
        Task<UserEntity> GetUserByIdAsync(string userId);
        Task<bool> VerifyUserOwnershipAsync(string userId, int taskId);
    }
}