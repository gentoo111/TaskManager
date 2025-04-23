namespace TaskManager.Core.Entities
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public int? Priority { get; set; }
        public string? UserId { get; set; }
    }
}