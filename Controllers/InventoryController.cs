using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LogiTrack.Data; // Ensure this matches the actual namespace in InventoryRepository.cs

namespace LogiTrack.Controllers
{
    [ApiController]
    [Route("api/v1/inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly InventoryRepository _repository;

        public InventoryController(AppDbContext db)
        {
            _repository = new InventoryRepository(db);
        }

        // GET: /api/v1/inventory
        [HttpGet]
        public async Task<ActionResult<List<InventoryItem>>> GetAll()
        {
            var items = await _repository.GetAllAsync();
            return Ok(items);
        }

        // GET: /api/v1/inventory/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> GetById(int id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null)
                return NotFound($"Inventory item with Id {id} was not found.");
            return Ok(item.DisplayInfo());
        }

        // POST: /api/v1/inventory
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> Add([FromBody] InventoryItem item)
        {
            if (item == null)
                return BadRequest();

            var exists = await _repository.ExistsAsync(item.Id);
            if (exists)
                return Conflict($"An item with Id {item.Id} already exists.");

            var addedItem = await _repository.AddAsync(item);
            return CreatedAtAction(nameof(GetAll), null, addedItem);
        }

        // DELETE: /api/v1/inventory/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteAsync(id);
            if (!deleted)
                 return NotFound($"Inventory item with Id {id} was not found.");

            return NoContent();
        }
    }
}
