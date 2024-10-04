using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WebAPI.Models;

public class TaskService
{
    private readonly string _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "task.json");

    // Method to get the next available Task ID
    private int GetNextTaskId()
    {
        var tasks = GetTasks();
        return tasks.Count > 0 ? tasks.Max(t => t.TaskId) + 1 : 1; // Start from 1 if no tasks exist
    }

    public List<UserTask> GetTasks()
    {
        if (!File.Exists(_jsonFilePath))
        {
            return new List<UserTask>();  // Return empty list if no file exists
        }

        var jsonData = File.ReadAllText(_jsonFilePath);
        return JsonSerializer.Deserialize<List<UserTask>>(jsonData) ?? new List<UserTask>();
    }

    public void AddTask(UserTask newTask)
    {
        var tasks = GetTasks();
        newTask.TaskId = GetNextTaskId();  // Automatically assign a new Task ID
        newTask.Status = "Planned";  // Default status
        tasks.Add(newTask);

        var updatedJsonData = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jsonFilePath, updatedJsonData);
    }

    public bool UpdateTaskStatus(int id, string newStatus)
    {
        var tasks = GetTasks();
        var taskToUpdate = tasks.Find(t => t.TaskId == id);
        if (taskToUpdate == null)
        {
            return false;
        }

        taskToUpdate.Status = newStatus;
        var updatedJsonData = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jsonFilePath, updatedJsonData);
        return true;
    }

    public bool DeleteTask(int id)
    {
        var tasks = GetTasks();
        var taskToDelete = tasks.Find(t => t.TaskId == id);
        if (taskToDelete == null)
        {
            return false;
        }

        tasks.Remove(taskToDelete);
        var updatedJsonData = JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jsonFilePath, updatedJsonData);
        return true;
    }

    public List<UserTask> GetTasksByUsername(string username)
    {
        var allTasks = GetTasks();  // Get all tasks from the JSON file
        return allTasks.Where(t => t.Username == username).ToList();  // Return tasks for the specified username
    }
}
