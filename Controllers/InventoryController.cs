using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiTrack.Data;
using LogiTrack.Models; // Add this line

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryRepository _repository;

        public InventoryController(IInventoryRepository repository)
        {
            _repository = repository;
        }

        // GET: /api/v1/inventory
        [HttpGet]
       // [Authorize]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetAll()
        {
            // Optionally measure performance
            // var sw = System.Diagnostics.Stopwatch.StartNew();
            var items = await _repository.GetAllAsync();
            // sw.Stop();
            // Console.WriteLine($"GetAll executed in {sw.ElapsedMilliseconds} ms");
            return Ok(items);
        }

        // GET: /api/v1/inventory/{id}
        [HttpGet("{id}")]
      // [Authorize]
        public async Task<ActionResult<InventoryItem>> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return NotFound($"Inventory item with Id {id} was not found.");
            return Ok(item);
        }

        // POST: /api/v1/inventory
        [HttpPost]
       // [Authorize(Roles = "Manager")]
        public async Task<ActionResult<InventoryItem>> Add([FromBody] InventoryItem item)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                if (item == null)
                    return BadRequest();

                var exists = await _repository.ExistsAsync(item.Id);
                if (exists)
                    return Conflict($"An item with Id {item.Id} already exists.");

                var addedItem = await _repository.AddAsync(item);
                return CreatedAtAction(nameof(GetAll), null, addedItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: /api/v1/inventory/{id}
        [HttpDelete("{id}")]
       // [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var deleted = await _repository.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Inventory item with Id {id} was not found.");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
