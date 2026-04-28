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
        // 1. Configurar base de datos en memoria para cada prueba
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);

        // 2. Sembrar datos de prueba (Seed data)
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

        // Primero verificamos que no sea nulo (esto quita el Warning)
        Assert.That(product, Is.Not.Null, "El producto debería existir en la DB");

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.TypeOf<OkObjectResult>(), "Debería retornar un Ok");
            Assert.That(product!.Stock, Is.EqualTo(7), "El stock debería haber bajado de 10 a 7");
        });
    }

    [TearDown]
    public void Dispose()
    {
        _context.Dispose();
    }
}