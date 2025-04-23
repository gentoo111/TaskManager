// Updated TasksController.cs file
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using TaskManager.Core.Entities;
using TaskManager.Core.Interfaces;

namespace TaskManager.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _taskRepository;
        private readonly ILogger<TasksController> _logger;

        public TasksController(ITaskRepository taskRepository, ILogger<TasksController> logger)
        {
            _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            try
            {
                // Simplified: In a real project, get user ID from authentication token
                string userId = "test-user";
                var tasks = await _taskRepository.GetAllTasksAsync(userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all tasks");
                return StatusCode(500, "Internal error occurred while fetching the task list");
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

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching task {TaskId}", id);
                return StatusCode(500, "Internal error occurred while fetching the task");
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

                // Set base properties
                task.UserId = "test-user"; // Simplified
                task.CreatedAt = DateTime.UtcNow;

                await _taskRepository.AddTaskAsync(task);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating task");
                return StatusCode(500, "Internal error occurred while creating the task");
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

                // Simple authorization check
                if (existingTask.UserId != "test-user")
                    return Forbid();

                // Retain original creation time
                task.CreatedAt = existingTask.CreatedAt;
                task.UserId = existingTask.UserId;

                await _taskRepository.UpdateTaskAsync(task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating task {TaskId}", id);
                return StatusCode(500, "Internal error occurred while updating the task");
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

                // Simple authorization check
                if (task.UserId != "test-user")
                    return Forbid();

                await _taskRepository.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting task {TaskId}", id);
                return StatusCode(500, "Internal error occurred while deleting the task");
            }
        }
    }
}
