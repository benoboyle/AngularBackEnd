using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WebAPI.Models;

public class UserService
{
    private readonly string _jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "users.json");
    private readonly string _loginFilePath = Path.Combine(Directory.GetCurrentDirectory(), "latestLogin.json");

    // Retrieve all users from the JSON file
    public List<User> GetAllUsers()
    {
        if (!File.Exists(_jsonFilePath))
        {
            return new List<User>();
        }

        var jsonData = File.ReadAllText(_jsonFilePath);
        var users = JsonSerializer.Deserialize<List<User>>(jsonData);
        return users ?? new List<User>();
    }

    // Check if a username is already taken
    public bool IsUsernameTaken(string username)
    {
        var users = GetAllUsers();
        return users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    // Hash password using HMACSHA512 with a salt
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using (var hmac = new HMACSHA512())
        {
            passwordSalt = hmac.Key; // Generates a random salt
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password)); // Hashes the password with the salt
        }
    }

    // Save a new user to the JSON file
    public void SaveUser(User user, string password)
    {
        if (IsUsernameTaken(user.Username))
        {
            throw new Exception("Username already taken.");
        }

        // Generate password hash and salt
        byte[] passwordHash, passwordSalt;
        CreatePasswordHash(password, out passwordHash, out passwordSalt);

        // Store the hashed password and salt in the user object
        user.PasswordHash = passwordHash;
        user.Salt = passwordSalt;

        var users = GetAllUsers();
        users.Add(user);

        var updatedJsonData = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_jsonFilePath, updatedJsonData);
    }

    // Verify the password during login using HMACSHA512
    public bool VerifyPasswordHash(string enteredPassword, byte[] storedHash, byte[] storedSalt)
    {
        using (var hmac = new HMACSHA512(storedSalt))
        {
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
            return computedHash.SequenceEqual(storedHash);
        }
    }

    // Verify user login and store the latest login
    public bool VerifyUserLogin(string username, string password)
    {
        var users = GetAllUsers();
        var user = users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

        if (user == null || !VerifyPasswordHash(password, user.PasswordHash, user.Salt))
        {
            return false;
        }

        // Store the latest login in latestLogin.json
        SaveLatestLogin(username);
        return true;
    }

    // Save the latest login username in latestLogin.json
    private void SaveLatestLogin(string username)
    {
        var loginData = new { username = username };
        var jsonData = JsonSerializer.Serialize(loginData, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_loginFilePath, jsonData);
    }

    // Retrieve the latest login username
    public string GetLatestLoginUsername()
    {
        if (File.Exists(_loginFilePath))
        {
            var jsonData = File.ReadAllText(_loginFilePath);
            var loginData = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);
            return loginData["username"]; // Make sure this matches the key in your JSON
        }
        return null;
    }

}
