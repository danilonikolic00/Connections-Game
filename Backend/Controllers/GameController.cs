using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Backend.Controllers;
[ApiController]
[Route("[controller]")]
public class GameController : ControllerBase
{
    public ConnectionsDBContext Context { get; set; }
    private IConfiguration _config;
    public GameController(ConnectionsDBContext context, IConfiguration config)
    {
        Context = context;
        _config = config;
    }

    [Route("GetFourGroups")]
    [HttpGet]
    public async Task<IActionResult> GetFourGroups()
    {
        int count = await Context.Groups.CountAsync();
        if (count < 4)
            return BadRequest("Not enough groups available.");

        var randomGroups = await Context.Groups
                                        .OrderBy(g => Guid.NewGuid())
                                        .Take(4)
                                        .ToListAsync();

        var groupIds = randomGroups.Select(g => g.GroupId).ToList();

        var terms = await Context.Terms
                                 .Where(t => groupIds.Contains(t.Group.GroupId))
                                 .ToListAsync();

        var result = randomGroups.Select(g => new
        {
            GroupId = g.GroupId,
            GroupName = g.GroupName,
            Terms = terms.Where(t => t.Group.GroupId == g.GroupId)
                         .Select(t => t.TermName)
                         .ToList()
        }).ToList();

        return Ok(result);
    }

    [Route("UpdateScore/{id}/{win}")]
    [HttpPut]
    public async Task<IActionResult> UpdateScore(int id, bool win)
    {
        try
        {
            var player = await Context.Players.Where(p => p.Id == id).FirstOrDefaultAsync();
            if (player == null)
            {
                return BadRequest("Player not found!");
            }
            if (win)
            {
                player.Solved += 1;
            }
            player.Played += 1;
            player.SuccessPercentage = Math.Round(player.Solved / player.Played * 100);
            await Context.SaveChangesAsync();
            return Ok(player);
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Route("GetPlayerStats/{id}")]
    [HttpGet]
    public async Task<IActionResult> GetPlayerStats(int id)
    {
        try
        {
            var user = await Context.Players.Where(p => p.Id == id)
                                            .Select(p => new
                                            {
                                                Played = p.Played,
                                                Solved = p.Solved,
                                                SuccessPercentage = p.SuccessPercentage
                                            })
                                            .FirstOrDefaultAsync();
            return Ok(user);
        }
        catch (Exception e)
        {
            return Ok(e.Message);
        }
    }

    [Route("CheckGuess/{guess1}/{guess2}/{guess3}/{guess4}")]
    [HttpGet]
    public async Task<IActionResult> CheckGuess(string guess1, string guess2, string guess3, string guess4)
    {
        try
        {
            var term = await Context.Terms
                                    .Where(t => t.TermName == guess1)
                                    .Select(t => new
                                    {
                                        GroupName = t.Group.GroupName,
                                        GroupId = t.Group.GroupId
                                    })
                                    .FirstOrDefaultAsync();
            if (term == null)
            {
                return BadRequest("Term not found!");
            }
            List<string> terms = Context.Terms.Where(t => t.Group.GroupId == term.GroupId).Select(t => t.TermName).ToList();
            string[] remain_guesses = { guess2, guess3, guess4 };

            foreach (var guess in remain_guesses)
            {
                if (!terms.Contains(guess))
                    return Ok("Group not guessed right!");
            }
            return Ok($"Group guessed right! {term.GroupName}");
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}