// 新增 SyncUserDto.cs
namespace TaskManager.Core.DTOs
{
    public class SyncUserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}