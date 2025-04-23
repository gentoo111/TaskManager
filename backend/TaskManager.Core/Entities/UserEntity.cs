using System;
using System.Collections.Generic;

namespace TaskManager.Core.Entities
{
    public class UserEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool EmailConfirmed { get; set; } = false;
        public string Role { get; set; } = "User"; // Default role
        
        // Navigation property
        public virtual ICollection<TaskItem>? Tasks { get; set; }
    }
}