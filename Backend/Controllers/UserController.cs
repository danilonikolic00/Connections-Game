using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Controllers;
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    public ConnectionsDBContext Context { get; set; }
    private IConfiguration _config;
    public UserController(ConnectionsDBContext context, IConfiguration config)
    {
        Context = context;
        _config = config;
    }

    private string GenerateJwtToken(User user)
    {

        var claims = new[]
        {
            new Claim(ClaimTypes.GivenName, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.NameIdentifier,user.Id.ToString())
        };

        string? jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new InvalidOperationException("JWT key is missing from configuration.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Route("Register/{username}/{password}")]
    [HttpPost]
    public async Task<IActionResult> Register(string username, string password)
    {
        try
        {
            if (await Context.Users.Where(p => p.Username == username).FirstOrDefaultAsync() != null)
                return Ok("Username exist!");
            var passwordHasher = new PasswordHasher<Player>();
            Player p = new Player
            {
                Username = username,
                Password = "",
                Role = "user",
                Played = 0,
                Solved = 0,
                SuccessPercentage = 0
            };
            p.Password = passwordHasher.HashPassword(p, password);
            Context.Players.Add(p);
            await Context.SaveChangesAsync();
            return Ok("Registration Successful!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("RegisterAdmin/{username}/{password}")]
    [HttpPost]
    public async Task<IActionResult> RegisterAdmin(string username, string password)
    {
        try
        {
            if (await Context.Users.Where(p => p.Username == username).FirstOrDefaultAsync() != null)
                return Ok("Username exist!");
            var passwordHasher = new PasswordHasher<User>();
            User u = new User
            {
                Username = username,
                Password = "",
                Role = "admin"
            };
            u.Password = passwordHasher.HashPassword(u, password);
            Context.Users.Add(u);
            await Context.SaveChangesAsync();
            return Ok("Admin Registration Successful!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("Login/{username}/{password}")]
    [HttpGet]
    public async Task<IActionResult> Login(string username, string password)
    {
        try
        {
            var user = await Context.Users.Where(u => u.Username == username).FirstOrDefaultAsync();
            if (user != null)
            {
                var passwordHasher = new PasswordHasher<User>();
                var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);
                if (result == PasswordVerificationResult.Success)
                {
                    var token = GenerateJwtToken(user);
                    return Ok(new { token });
                }
            }
            return Unauthorized("Invalid credentials!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("ChangePassword/{id}/{password}/{new_password}")]
    [HttpPut]
    public async Task<ActionResult> ChangePassword(int id, string password, string new_password)
    {
        try
        {
            var user = Context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return BadRequest("User not found!");
            }

            var passwordHasher = new PasswordHasher<User>();
            var result = passwordHasher.VerifyHashedPassword(user, user.Password, password);

            if (result == PasswordVerificationResult.Failed)
            {
                return BadRequest("Wrong password entered!");
            }

            string hashedPassword = passwordHasher.HashPassword(user, new_password);
            user.Password = hashedPassword;
            await Context.SaveChangesAsync();

            return Ok("Password changed successfully!");
        }
        catch (Exception e)
        {
            return Ok(e.Message);
        }
    }
}