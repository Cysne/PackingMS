namespace PackingService.Api.Entities
{
    public class ProductEntity
    {
        public int ProductId { get; set; }
        public string? Name { get; set; }
        public decimal Height { get; set; }
        public decimal Width { get; set; }
        public decimal Length { get; set; }
    }
}