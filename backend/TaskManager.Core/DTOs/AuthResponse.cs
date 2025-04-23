// AuthResponse.cs
namespace TaskManager.Core.DTOs
{
    public class AuthResponse
    {
        public bool Successful { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Message { get; set; }
    }
}