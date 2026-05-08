using Amazon.SQS;
using Amazon.SQS.Model;
using Inventory.API.Data;
using Microsoft.EntityFrameworkCore;

namespace Inventory.OutboxProcessor;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IAmazonSQS _sqsClient;
    private readonly IConfiguration _configuration;
    private string _queueUrl = string.Empty;

    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory, IAmazonSQS sqsClient, IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _sqsClient = sqsClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Asegurarnos de que la cola existe en LocalStack
        var queueName = _configuration["AWS:QueueName"];
        var createQueueResponse = await _sqsClient.CreateQueueAsync(queueName, stoppingToken);
        _queueUrl = createQueueResponse.QueueUrl;

        _logger.LogInformation("Conectado a la cola SQS en LocalStack: {QueueUrl}", _queueUrl);

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var events = await context.IntegrationEventOutboxes
                    .Where(e => e.ProcessedOnUtc == null)
                    .OrderBy(e => e.OccurredOnUtc)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var outboxEvent in events)
                {
                    try
                    {
                        var sendMessageRequest = new SendMessageRequest
                        {
                            QueueUrl = _queueUrl,
                            MessageBody = outboxEvent.Content
                            // MessageGroupId eliminado
                        };

                        await _sqsClient.SendMessageAsync(sendMessageRequest, stoppingToken);

                        _logger.LogInformation("Evento {Id} enviado con éxito a SQS.", outboxEvent.Id);

                        outboxEvent.ProcessedOnUtc = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error enviando evento {Id} a SQS", outboxEvent.Id);
                    }
                }

                await context.SaveChangesAsync(stoppingToken);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}