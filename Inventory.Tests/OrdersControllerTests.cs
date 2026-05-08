using Inventory.API.Controllers;
using Inventory.API.Data;
using Inventory.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Inventory.Tests;

[TestFixture]
public class OrdersControllerTests
{
    private AppDbContext _context;
    private OrdersController _controller;

    [SetUp]
    public void Setup()
    {
        // Configurar base de datos en memoria para cada prueba
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // Datos de prueba
        _context.Products.Add(new Product { Id = 1, Name = "Laptop", Stock = 10 });
        _context.SaveChanges();

        _controller = new OrdersController(_context);
    }

    [Test]
    public async Task CreateOrder_ShouldReduceStock_WhenStockIsAvailable()
    {
        // Arrange (Preparar)
        var order = new Order { ProductId = 1, Quantity = 3 };

        // Act (Actuar)
        var result = await _controller.CreateOrder(order);

        // Assert (Afirmar)
        var product = await _context.Products.FindAsync(1);

        // Verificamos que no sea nulo (esto quita el Warning)
        Assert.That(product, Is.Not.Null, "El producto debería existir en la DB");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>(), "Debería retornar un Ok");
            Assert.That(product!.Stock, Is.EqualTo(7), "El stock debería haber bajado de 10 a 7");
        });
    }

    [Test]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenStockIsInsufficient()
    {
        // Arrange: Intentamos comprar 15, pero el Seed Data solo tiene 10
        var order = new Order { ProductId = 1, Quantity = 15 };

        // Act
        var result = await _controller.CreateOrder(order);

        // Assert
        var product = await _context.Products.FindAsync(1);

        Assert.Multiple(() =>
        {
            // Debe rechazar la petición
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>(), "Debería retornar BadRequest");

            // El stock NO debe descontarse (debe seguir en 10)
            Assert.That(product!.Stock, Is.EqualTo(10), "El stock no debe cambiar por un pedido fallido");

            // No debe existir ningún evento en el Outbox por un error
            Assert.That(_context.IntegrationEventOutboxes.Count(), Is.EqualTo(0), "No se debe crear evento en Outbox");
        });
    }

    [Test]
    public async Task CreateOrder_ShouldReturnBadRequest_WhenQuantityIsInvalid()
    {
        // Arrange: Intentamos hackear el sistema con una cantidad negativa
        var order = new Order { ProductId = 1, Quantity = -5 };

        // Act
        var result = await _controller.CreateOrder(order);

        // Assert
        var product = await _context.Products.FindAsync(1);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>(), "Debería retornar BadRequest para cantidades negativas");
            Assert.That(product!.Stock, Is.EqualTo(10), "El sistema no debe sumar stock por error");
        });
    }

    [Test]
    public async Task CreateOrder_ShouldCreateOutboxEvent_WhenOrderIsSuccessful()
    {
        // Arrange: Un pedido normal y válido
        var order = new Order { ProductId = 1, Quantity = 2 };

        // Act
        await _controller.CreateOrder(order);

        // Assert
        var outboxEvents = await _context.IntegrationEventOutboxes.ToListAsync();

        Assert.Multiple(() =>
        {
            // Debe existir exactamente un evento esperando ser procesado
            Assert.That(outboxEvents.Count, Is.EqualTo(1), "Debe existir exactamente un evento en el Outbox");

            // El evento debe tener los datos correctos
            Assert.That(outboxEvents.First().EventType, Is.EqualTo("OrderCreated"), "El tipo de evento debe ser OrderCreated");

            // Su fecha de procesamiento debe ser nula (porque el Worker aún no lo toca)
            Assert.That(outboxEvents.First().ProcessedOnUtc, Is.Null, "El evento debe estar marcado como NO procesado (null)");
        });
    }

    [TearDown]
    public void Dispose()
    {
        _context.Dispose();
    }
}