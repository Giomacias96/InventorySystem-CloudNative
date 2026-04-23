namespace Inventory.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
        public DateTime LastUpdate { get; set; } = DateTime.Now;
    }
}