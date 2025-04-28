// VerifyAccountDto.cs
namespace TaskManager.Core.DTOs
{
    public class VerifyAccountDto
    {
        public string Username { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}