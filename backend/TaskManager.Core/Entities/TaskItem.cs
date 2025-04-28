using System.Text.Json.Serialization;

namespace TaskManager.Core.Entities
{
    public class TaskItem
    {
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime? _dueDate;
        
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set => _createdAt = DateTime.SpecifyKind(value, DateTimeKind.Utc); 
        }
        public DateTime? DueDate 
        { 
            get => _dueDate; 
            set => _dueDate = value.HasValue ? 
                DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : null; 
        }
        public bool IsCompleted { get; set; } = false;
        public int? Priority { get; set; }
        
        // Add these properties
        public string UserId { get; set; } = string.Empty;
        
        // Navigation property
        [JsonIgnore]
        public virtual UserEntity? User { get; set; }
    }
}