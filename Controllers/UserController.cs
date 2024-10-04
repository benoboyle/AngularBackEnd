using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using WebAPI.Models;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController()
    {
        _userService = new UserService();
    }

    // GET: api/user
    [HttpGet]
    public ActionResult<List<User>> GetUsers()
    {
        var users = _userService.GetAllUsers();
        return Ok(users);
    }

    // POST: api/user
    [HttpPost]
    public ActionResult CreateUser([FromBody] UserDTO userDto) // Use UserDTO to receive raw password and username
    {
        if (string.IsNullOrEmpty(userDto.Username) || string.IsNullOrEmpty(userDto.Password))
        {
            return BadRequest("Username and password are required.");
        }

        try
        {
            // Create a new User object and pass the password separately
            User newUser = new User
            {
                Username = userDto.Username,
            };

            _userService.SaveUser(newUser, userDto.Password); // Pass the raw password separately
            return Ok(new { message = "User created successfully" });
        }
        catch (Exception ex)
        {
            if (ex.Message == "Username already taken.")
            {
                return Conflict("Username is already taken.");
            }

            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    // GET: api/user/check-username?username=<username>
    [HttpGet("check-username")]
    public ActionResult<bool> CheckUsername(string username)
    {
        if (string.IsNullOrEmpty(username))
        {
            return BadRequest("Username is required.");
        }

        var isTaken = _userService.IsUsernameTaken(username);
        return Ok(isTaken);
    }

    // POST: api/user/login
    [HttpPost("login")]
    public ActionResult Login([FromBody] UserLoginDto loginDto)
    {
        if (string.IsNullOrEmpty(loginDto.Username) || string.IsNullOrEmpty(loginDto.Password))
        {
            return BadRequest("Username and password are required.");
        }

        var isLoginSuccessful = _userService.VerifyUserLogin(loginDto.Username, loginDto.Password);

        if (isLoginSuccessful)
        {
            // Store the last login information
            var lastLoginFilePath = Path.Combine(Directory.GetCurrentDirectory(), "latestLogin.json");
            var loginData = new Dictionary<string, string>
            {
                { "username", loginDto.Username } // Change to lowercase
            };

            var json = JsonSerializer.Serialize(loginData);
            System.IO.File.WriteAllText(lastLoginFilePath, json);

            return Ok(new { message = "Login successful" });
        }
        else
        {
            return Unauthorized("Invalid username or password.");
        }
    }

    // GET: api/user/latest-login
    [HttpGet("latest-login")]
    public ActionResult<string> GetLastLoginUser()
    {
        var lastLoginFilePath = Path.Combine(Directory.GetCurrentDirectory(), "latestLogin.json");

        if (!System.IO.File.Exists(lastLoginFilePath))
        {
            return NotFound("No last login found.");
        }

        var jsonData = System.IO.File.ReadAllText(lastLoginFilePath);
        var lastLogin = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonData);

        if (lastLogin != null && lastLogin.ContainsKey("username")) 
        {
            return Ok(lastLogin["username"]); 
        }

        return NotFound("No last login found.");
    }
}
