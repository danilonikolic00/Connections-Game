using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;
// [Authorize(Roles = "Admin")]
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase
{
    public ConnectionsDBContext Context { get; set; }
    private IConfiguration _config;
    public AdminController(ConnectionsDBContext context, IConfiguration config)
    {
        Context = context;
        _config = config;
    }

    [Route("AddGroup/{group_name}/{term1}/{term2}/{term3}/{term4}")]
    [HttpPost]
    public async Task<IActionResult> AddGroup(string group_name, string term1, string term2, string term3, string term4)
    {
        try
        {
            var existing = Context.Groups.Where(g => g.GroupName == group_name).FirstOrDefault();
            if (existing != null)
                return BadRequest("Group with this name already exist!");
            Group g = new()
            {
                GroupName = group_name
            };
            Context.Groups.Add(g);
            string[] terms = { term1, term2, term3, term4 };
            foreach (var term in terms)
            {
                Term t = new()
                {
                    TermName = term,
                    Group = g
                };
                Context.Terms.Add(t);
            }
            await Context.SaveChangesAsync();
            return Ok("Group added successfuly!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("GetAllPlayers")]
    [HttpGet]
    public async Task<ActionResult> GetAllPlayers()
    {
        try
        {
            var players = await Context.Players.Select(p => new {
                Id = p.Id,
                Username = p.Username,
                Played = p.Played,
                Solved = p.Solved,
                SuccessPecrentage = p.SuccessPercentage
            }).ToListAsync();
            if(players == null)
                return BadRequest("Players not found!");
            return Ok(players);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("Deleteplayer/{id}")]
    [HttpDelete]
    public async Task<ActionResult> RemovePlayer(int id)
    {
        try
        {
            var player = await Context.Players.Where(p=>p.Id ==id).FirstOrDefaultAsync();
            if(player == null)
                return BadRequest("Player not found!");
            Context.Players.Remove(player);
            await Context.SaveChangesAsync();
            return Ok("Player removed successfully!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("GetAllGroups")]
    [HttpGet]
    public async Task<ActionResult> GetAllGroups()
    {
        try
        {
            var groups = await Context.Groups.ToListAsync();

            var groupIds = groups.Select(g => g.GroupId).ToList();

            var terms = await Context.Terms
                                 .Where(t => groupIds.Contains(t.Group.GroupId))
                                 .ToListAsync();

            var result = groups.Select(g => new
            {
                GroupId = g.GroupId,
                GroupName = g.GroupName,
                Terms = terms.Where(t => t.Group.GroupId == g.GroupId)
                         .Select(t => t.TermName)
                         .ToList()
            }).ToList();

            return Ok(result);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("DeleteGroup/{id}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteGroup(int id)
    {
        try
        {
            var group = await Context.Groups.Where(g => g.GroupId == id).FirstOrDefaultAsync();
            if(group == null)
                return BadRequest("Group not found!");
            Context.Groups.Remove(group);
            await Context.SaveChangesAsync();
            return Ok("Group deleted successfully!");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}