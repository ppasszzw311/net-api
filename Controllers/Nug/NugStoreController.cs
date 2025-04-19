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
  public class StoreController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public StoreController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: nug/Store
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NugStore>>> GetStores()
    {
      return await _context.NugStores
          .Include(s => s.Owner)
          .ToListAsync();
    }

    // GET: nug/Store/5
    [HttpGet("{uuid}")]
    public async Task<ActionResult<NugStore>> GetStore(Guid uuid)
    {
      var store = await _context.NugStores
          .Include(s => s.Owner)
          .FirstOrDefaultAsync(s => s.UUID == uuid);

      if (store == null)
      {
        return NotFound();
      }

      return store;
    }

    // GET: nug/Store/owner/{ownerId}
    [HttpGet("owner/{ownerId}")]
    public async Task<ActionResult<IEnumerable<NugStore>>> GetStoresByOwner(string ownerId)
    {
      return await _context.NugStores
          .Include(s => s.Owner)
          .Where(s => s.OwnerId == ownerId)
          .ToListAsync();
    }

    // POST: nug/Store
    [HttpPost]
    public async Task<ActionResult<NugStore>> CreateStore(NugStore store)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      _context.NugStores.Add(store);
      await _context.SaveChangesAsync();

      return CreatedAtAction(
          nameof(GetStore),
          new { uuid = store.UUID },
          store
      );
    }

    // PUT: nug/Store/5
    [HttpPut("{uuid}")]
    public async Task<IActionResult> UpdateStore(Guid uuid, NugStore store)
    {
      if (uuid != store.UUID)
      {
        return BadRequest();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        _context.Entry(store).State = EntityState.Modified;
        // 防止更新 CreatedAt
        _context.Entry(store).Property(x => x.CreatedAt).IsModified = false;
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!StoreExists(uuid))
        {
          return NotFound();
        }
        throw;
      }

      return NoContent();
    }

    // DELETE: nug/Store/5
    [HttpDelete("{uuid}")]
    public async Task<IActionResult> DeleteStore(Guid uuid)
    {
      var store = await _context.NugStores.FindAsync(uuid);
      if (store == null)
      {
        return NotFound();
      }

      _context.NugStores.Remove(store);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool StoreExists(Guid uuid)
    {
      return _context.NugStores.Any(e => e.UUID == uuid);
    }
  }
}