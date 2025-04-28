// Update the TasksController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Add authorization to all endpoints
    public class TasksController : BaseApiController
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<TasksController> _logger;

        public TasksController(
            ITaskRepository taskRepository, 
            IAuthService authService,
            ILogger<TasksController> logger) : base(logger)
        {
            _taskRepository = taskRepository;
            _authService = authService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            try
            {
                string userId = GetUserId();
                _logger.LogInformation("Looking for tasks with UserId: {UserId}", userId);
        
                // Before filtering, log all tasks
                var allTasks = await _taskRepository.GetAllTasksAsync(null); // Temporarily get all tasks
                _logger.LogInformation("All tasks count: {Count}", allTasks.Count());
                foreach (var task in allTasks)
                {
                    _logger.LogInformation("Task ID: {TaskId}, UserId: {UserId}", task.Id, task.UserId);
                }
        
                // Then get filtered tasks
                var userTasks = await _taskRepository.GetAllTasksAsync(userId);
                _logger.LogInformation("Found {Count} tasks for user", userTasks.Count());
        
                return Ok(userTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving tasks");
                return StatusCode(500, "An error occurred while retrieving tasks");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound($"Task with ID {id} not found");

                // Verify ownership
                string userId = GetUserId();
                if (task.UserId != userId)
                    return Forbid();

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving task {TaskId}", id);
                return StatusCode(500, "An error occurred while retrieving the task");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            try
            {
                if (task == null)
                    return BadRequest("Task data cannot be null");

                if (string.IsNullOrEmpty(task.Title))
                    return BadRequest("Task title cannot be empty");

                // Set user ID
                string userId = GetUserId();
                _logger.LogInformation("Creating task with UserId: {UserId}", task.UserId);
                
                var user = await _authService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found in database: {UserId}", userId);
                    return BadRequest("User not found. Please logout and login again.");
                }

                task.UserId = userId;
                
                task.CreatedAt = DateTime.UtcNow;
                
                if (task.DueDate.HasValue)
                {
                    task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);
                }

                _logger.LogInformation("Task details: {Task}", 
                    new { task.Title, task.Description, task.DueDate, task.Priority });
                await _taskRepository.AddTaskAsync(task);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "详细错误信息: 创建任务失败 - {Message}", ex.Message);
        
                if (ex.InnerException != null)
                {
                    _logger.LogError("内部异常: {InnerMessage}", ex.InnerException.Message);
                }
        
                return StatusCode(500, $"创建任务失败: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            try
            {
                if (id != task.Id)
                    return BadRequest("Path ID does not match task ID");

                var existingTask = await _taskRepository.GetTaskByIdAsync(id);
                if (existingTask == null)
                    return NotFound($"Task with ID {id} not found");

                // Verify ownership
                string userId = GetUserId();
                if (existingTask.UserId != userId)
                    return Forbid();

                // Preserve original creation time and user ID
                task.CreatedAt = existingTask.CreatedAt;
                task.UserId = userId;
                
                if (task.DueDate.HasValue)
                {
                    task.DueDate = DateTime.SpecifyKind(task.DueDate.Value, DateTimeKind.Utc);
                }

                await _taskRepository.UpdateTaskAsync(task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating task {TaskId}", id);
                return StatusCode(500, "An error occurred while updating the task");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound($"Task with ID {id} not found");

                // Verify ownership
                string userId = GetUserId();
                if (task.UserId != userId)
                    return Forbid();

                await _taskRepository.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting task {TaskId}", id);
                return StatusCode(500, "An error occurred while deleting the task");
            }
        }
    }
}