using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: api/tasks
        [HttpGet]
        public ActionResult<IEnumerable<UserTask>> GetTasks()
        {
            var lastLoginFilePath = Path.Combine(Directory.GetCurrentDirectory(), "latestLogin.json");

            if (!System.IO.File.Exists(lastLoginFilePath))
            {
                return NotFound("No last login found.");
            }

            var jsonData = System.IO.File.ReadAllText(lastLoginFilePath);
            var lastLogin = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

            if (lastLogin != null && lastLogin.TryGetValue("username", out var username)) // Note the lowercase
            {
                var userTasks = _taskService.GetTasksByUsername(username);

                if (userTasks == null || !userTasks.Any())
                {
                    return NotFound("No tasks found for this user.");
                }

                return Ok(userTasks);
            }

            return NotFound("Username not found in last login data.");
        }

        // POST: api/tasks
        [HttpPost]
        public IActionResult AddTask([FromBody] UserTask newTask)
        {
            if (newTask == null)
            {
                return BadRequest("Task data is missing.");
            }

            // Read the last login file to assign the username
            var lastLoginFilePath = Path.Combine(Directory.GetCurrentDirectory(), "latestLogin.json");

            if (!System.IO.File.Exists(lastLoginFilePath))
            {
                return NotFound("No last login found.");
            }

            var jsonData = System.IO.File.ReadAllText(lastLoginFilePath);
            var lastLogin = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

            if (lastLogin != null && lastLogin.TryGetValue("username", out var username))
            {
                newTask.Username = username; // Automatically assign the username from last login
                _taskService.AddTask(newTask); // Add the task
                return Ok(newTask); // Return the created task
            }

            return NotFound("Username not found in last login data.");
        }

        // PUT: api/tasks/{id}/status
        [HttpPut("{id}/status")]
        public IActionResult UpdateTaskStatus(int id, [FromBody] string newStatus)
        {
            var result = _taskService.UpdateTaskStatus(id, newStatus);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteTask(int id)
        {
            var result = _taskService.DeleteTask(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
