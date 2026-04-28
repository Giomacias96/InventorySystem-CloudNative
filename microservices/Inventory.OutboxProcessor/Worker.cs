using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.OutboxProcessor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Revisando tabla Outbox...");

            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Buscar eventos no procesados
                var events = await context.IntegrationEventOutboxes
                    .Where(e => e.ProcessedOnUtc == null)
                    .Take(10) // Procesamos de 10 en 10
                    .ToListAsync(stoppingToken);

                foreach (var outboxEvent in events)
                {
                    try
                    {
                        // Simular envío a AWS SQS
                        _logger.LogInformation("Enviando evento {Id} a AWS SQS: {Content}",
                            outboxEvent.Id, outboxEvent.Content);

                        // Aquí iría el código real de AWS SDK en el futuro
                        await Task.Delay(500, stoppingToken);

                        outboxEvent.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error procesando evento {Id}", outboxEvent.Id);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }

            // Esperar 5 segundos antes de la siguiente revisión
            await Task.Delay(5000, stoppingToken);
        }
    }
}