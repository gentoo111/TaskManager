// AuthResponse.cs
namespace TaskManager.Core.DTOs
{
    public class AuthResponse
    {
        public bool Successful { get; set; }
        public string Token { get; set; }= string.Empty;
        public string UserId { get; set; }= string.Empty;
        public string Username { get; set; }= string.Empty;
        public string Message { get; set; }= string.Empty;
    }
}