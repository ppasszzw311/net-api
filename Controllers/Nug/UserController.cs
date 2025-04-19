using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Models.Nug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NET_API.Controllers.Nug
{
  [ApiController]
  [Route("nug/[controller]")]
  public class UserController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public UserController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: nug/User
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NugUser>>> GetUsers()
    {
      return await _context.NugUsers.ToListAsync();
    }

    // GET: nug/User/5
    [HttpGet("{id}")]
    public async Task<ActionResult<NugUser>> GetUser(string id)
    {
      var user = await _context.NugUsers.FirstOrDefaultAsync(u => u.Id == id);

      if (user == null)
      {
        return NotFound();
      }

      return user;
    }

    // POST: nug/User
    [HttpPost]
    public async Task<ActionResult<NugUser>> CreateUser(NugUser user)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      _context.NugUsers.Add(user);
      await _context.SaveChangesAsync();

      return CreatedAtAction(
          nameof(GetUser),
          new { id = user.Id },
          user
      );
    }

    // PUT: nug/User/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, NugUser user)
    {
      if (id != user.Id)
      {
        return BadRequest();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        _context.Entry(user).State = EntityState.Modified;
        // 防止更新 CreatedAt
        _context.Entry(user).Property(x => x.CreateAt).IsModified = false;
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!UserExists(id))
        {
          return NotFound();
        }
        throw;
      }

      return NoContent();
    }

    // DELETE: nug/User/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
      var user = await _context.NugUsers.FirstOrDefaultAsync(u => u.Id == id);
      if (user == null)
      {
        return NotFound();
      }

      _context.NugUsers.Remove(user);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool UserExists(string id)
    {
      return _context.NugUsers.Any(e => e.Id == id);
    }
  }
}