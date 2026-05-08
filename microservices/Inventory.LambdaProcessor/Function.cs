using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Text.Json;

// Esto le dice a AWS cómo serializar los datos que entran a la función
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace Inventory.LambdaProcessor;

public class Function
{
    private readonly IAmazonDynamoDB _dynamoClient;
    private const string TableName = "InventorySummary";

    public Function()
    {
        // Configuración para LocalStack
        //var config = new AmazonDynamoDBConfig { ServiceURL = "http://host.docker.internal:4566" };
        var config = new AmazonDynamoDBConfig { ServiceURL = "http://localhost.localstack.cloud:4566" };
        _dynamoClient = new AmazonDynamoDBClient(new Amazon.Runtime.BasicAWSCredentials("test", "test"), config);
    }

    /// <summary>
    /// Esta función se "despierta" automáticamente cuando llegan mensajes a SQS
    /// </summary>
    /// <param name="evnt">El lote de mensajes que SQS nos entrega</param>
    /// <param name="context">El contexto de ejecución de AWS</param>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach (var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        context.Logger.LogInformation($"[NUEVO MENSAJE] Procesando ID de SQS: {message.MessageId}");
        context.Logger.LogInformation($"[CONTENIDO]: {message.Body}");

        try
        {
            // Deserializamos el JSON que mandó el Worker
            var orderData = JsonSerializer.Deserialize<OrderEventDto>(message.Body);

            if (orderData != null)
            {
                context.Logger.LogInformation($"Preparando para actualizar inventario en DynamoDB para el Producto ID: {orderData.ProductId}");

                // Lógica de "UpdateItem": Si el producto no existe, lo crea; si existe, suma/resta al stock.
                var request = new UpdateItemRequest
                {
                    TableName = TableName,
                    Key = new Dictionary<string, AttributeValue> { { "ProductId", new AttributeValue { N = orderData.ProductId.ToString() } } },
                    UpdateExpression = "ADD TotalSales :val SET LastUpdate = :time",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    { ":val", new AttributeValue { N = orderData.Quantity.ToString() } },
                    { ":time", new AttributeValue { S = DateTime.UtcNow.ToString("O") } }
                }
                };

                await _dynamoClient.UpdateItemAsync(request);
                context.Logger.LogInformation("Stock actualizado en DynamoDB exitosamente.");

                //await Task.CompletedTask;
            }
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Error procesando el mensaje: {ex.Message}");
            throw; // Lanzar el error hace que SQS sepa que fallamos y lo reintente después
        }
    }
}

// Un DTO (Data Transfer Object) para leer el JSON
public class OrderEventDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; }
}