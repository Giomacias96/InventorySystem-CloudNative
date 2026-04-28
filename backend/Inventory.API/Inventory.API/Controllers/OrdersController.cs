using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventory.API.Data;
using Inventory.API.Models;
using System.Text.Json;

namespace Inventory.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(Order order)
        {
            // Solo inicia la transacción si el proveedor no es In-Memory
            using var transaction = _context.Database.IsInMemory()
                ? null
                : await _context.Database.BeginTransactionAsync();

            try
            {
                if (order.Quantity <= 0)
                {
                    return BadRequest("La cantidad debe ser mayor a cero.");
                }

                // Verificar si hay stock suficiente
                var product = await _context.Products.FindAsync(order.ProductId);
                if (product == null || product.Stock < order.Quantity)
                {
                    return BadRequest("Stock insuficiente o producto no encontrado.");
                }

                product.Stock -= order.Quantity;
                product.LastUpdate = DateTime.UtcNow;

                // Registrar el pedido
                order.OrderDate = DateTime.UtcNow;
                _context.Orders.Add(order);

                // Crear el evento para el Outbox (Patrón Outbox)
                var outboxEvent = new IntegrationEventOutbox
                {
                    Id = Guid.NewGuid(),
                    EventType = "OrderCreated",
                    OccurredOnUtc = DateTime.UtcNow,
                    Content = JsonSerializer.Serialize(new { order.ProductId, order.Quantity, order.OrderDate }),
                    ProcessedOnUtc = null
                };
                _context.IntegrationEventOutboxes.Add(outboxEvent);

                await _context.SaveChangesAsync();
                if (transaction != null)
                    await transaction.CommitAsync();

                return Ok(new { Message = "Pedido creado y stock actualizado.", OrderId = order.Id });
            }
            catch (Exception ex)
            {
                // Si algo falla, se hace un Rollback automático al salir del bloque 'using'
                if (transaction != null)
                    await transaction.RollbackAsync();
                return StatusCode(500, $"Error interno: {ex.Message}");
            }
        }
    }
}