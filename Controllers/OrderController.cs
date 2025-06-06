using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using LogiTrack.Data;
using Microsoft.AspNetCore.Authorization;

namespace LogiTrack.Controllers
{   [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IInventoryRepository _repository;

        public OrderController(IInventoryRepository repository)
        {
            _repository = repository;
        }

        // GET: /api/v1/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetAll()
        {
            // Optionally measure performance
            // var sw = System.Diagnostics.Stopwatch.StartNew();
            var orders = await _repository.GetAllOrdersAsync();
            // sw.Stop();
            // Console.WriteLine($"GetAllOrders executed in {sw.ElapsedMilliseconds} ms");
            return Ok(orders);
        }

        // GET: /api/v1/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetById(int id)
        {
            var order = await _repository.GetOrderByIdAsync(id);
            if (order == null)
                return NotFound($"Order with Id {id} was not found.");
            return Ok(order);
        }

        // POST: /api/v1/orders
        [HttpPost]
        public async Task<ActionResult<Order>> Create([FromBody] Order order)
        {
            if (order == null)
                return BadRequest();

            var existing = await _repository.GetOrderByIdAsync(order.OrderId);
            if (existing != null)
                return Conflict($"An order with Id {order.OrderId} already exists.");

            var created = await _repository.AddOrderAsync(order);
            return CreatedAtAction(nameof(GetById), new { id = created.OrderId }, created);
        }

        // DELETE: /api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _repository.DeleteOrderAsync(id);
            if (!deleted)
                return NotFound($"Order with Id {id} was not found.");

            return NoContent();
        }
    }
}
