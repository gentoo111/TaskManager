using TaskManager.Core.Entities;

namespace TaskManager.Core.Interfaces
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TaskItem>> GetAllTasksAsync(string userId);
        Task<TaskItem?> GetTaskByIdAsync(int id);
        Task<TaskItem> AddTaskAsync(TaskItem task);
        Task UpdateTaskAsync(TaskItem task);
        Task DeleteTaskAsync(int id);
    }
}