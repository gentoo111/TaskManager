// 更新 TasksController.cs 文件
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
                // 简化版：在实际项目中，应从认证令牌获取用户ID
                string userId = "test-user";
                var tasks = await _taskRepository.GetAllTasksAsync(userId);
                return Ok(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有任务时出错");
                return StatusCode(500, "获取任务列表时发生内部错误");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound($"ID为{id}的任务不存在");

                return Ok(task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取任务 {TaskId} 时出错", id);
                return StatusCode(500, "获取任务时发生内部错误");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItem task)
        {
            try
            {
                if (task == null)
                    return BadRequest("任务数据不能为空");

                if (string.IsNullOrEmpty(task.Title))
                    return BadRequest("任务标题不能为空");

                // 设置基本属性
                task.UserId = "test-user"; // 简化版
                task.CreatedAt = DateTime.UtcNow;

                await _taskRepository.AddTaskAsync(task);
                return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建任务时出错");
                return StatusCode(500, "创建任务时发生内部错误");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskItem task)
        {
            try
            {
                if (id != task.Id)
                    return BadRequest("路径ID与任务ID不匹配");

                var existingTask = await _taskRepository.GetTaskByIdAsync(id);
                if (existingTask == null)
                    return NotFound($"ID为{id}的任务不存在");

                // 简单的授权检查
                if (existingTask.UserId != "test-user")
                    return Forbid();

                // 保留原始创建时间
                task.CreatedAt = existingTask.CreatedAt;
                task.UserId = existingTask.UserId;

                await _taskRepository.UpdateTaskAsync(task);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新任务 {TaskId} 时出错", id);
                return StatusCode(500, "更新任务时发生内部错误");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            try
            {
                var task = await _taskRepository.GetTaskByIdAsync(id);
                if (task == null)
                    return NotFound($"ID为{id}的任务不存在");

                // 简单的授权检查
                if (task.UserId != "test-user")
                    return Forbid();

                await _taskRepository.DeleteTaskAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除任务 {TaskId} 时出错", id);
                return StatusCode(500, "删除任务时发生内部错误");
            }
        }
    }
}