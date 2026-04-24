namespace Inventory.API.Models
{
    public class IntegrationEventOutbox
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty; // Ejemplo: "OrderCreated"
        public string Content { get; set; } = string.Empty;   // JSON con los datos del pedido
        public DateTime OccurredOnUtc { get; set; }
        public DateTime? ProcessedOnUtc { get; set; }        // Nulo si no se ha enviado a AWS SQS
    }
}