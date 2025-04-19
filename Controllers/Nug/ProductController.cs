using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Models.Nug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NET_API.Controllers
{
  [ApiController]
  [Route("nug/[controller]")]
  public class ProductController : ControllerBase
  {
    private readonly ApplicationDbContext _context;

    public ProductController(ApplicationDbContext context)
    {
      _context = context;
    }

    // GET: nug/Product
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NugProduct>>> GetProducts()
    {
      return await _context.NugProducts
          .Include(p => p.Store)
          .ToListAsync();
    }

    // GET: nug/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<NugProduct>> GetProduct(int id)
    {
      var product = await _context.NugProducts
          .Include(p => p.Store)
          .FirstOrDefaultAsync(p => p.Id == id);

      if (product == null)
      {
        return NotFound();
      }

      return product;
    }

    // GET: nug/Product/store/{storeId}
    [HttpGet("store/{storeId}")]
    public async Task<ActionResult<IEnumerable<NugProduct>>> GetProductsByStore(string storeId)
    {
      return await _context.NugProducts
          .Include(p => p.Store)
          .Where(p => p.StoreId == storeId)
          .ToListAsync();
    }

    // POST: nug/Product
    [HttpPost]
    public async Task<ActionResult<NugProduct>> CreateProduct(NugProduct product)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      _context.NugProducts.Add(product);
      await _context.SaveChangesAsync();

      return CreatedAtAction(
          nameof(GetProduct),
          new { id = product.Id },
          product
      );
    }

    // PUT: nug/Product/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, NugProduct product)
    {
      if (id != product.Id)
      {
        return BadRequest();
      }

      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      try
      {
        _context.Entry(product).State = EntityState.Modified;
        // 防止更新 CreatedAt
        _context.Entry(product).Property(x => x.CreatedAt).IsModified = false;
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!ProductExists(id))
        {
          return NotFound();
        }
        throw;
      }

      return NoContent();
    }

    // DELETE: nug/Product/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
      var product = await _context.NugProducts.FindAsync(id);
      if (product == null)
      {
        return NotFound();
      }

      _context.NugProducts.Remove(product);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    private bool ProductExists(int id)
    {
      return _context.NugProducts.Any(e => e.Id == id);
    }
  }
}
