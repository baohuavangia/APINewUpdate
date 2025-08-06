namespace MenShop_Assignment.DTOs
{
    public class ProductDTO
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public bool? Status { get; set; }
    }
}
