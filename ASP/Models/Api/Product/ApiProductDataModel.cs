namespace ASP.Models.Api.Product
{
    public class ApiProductDataModel
    {
        public String Name { get; set; } = null!;
        public String? ImageUrl { get; set; }
        public String? Description { get; set; }
        public String? Slug { get; set; }
        public String GroupId { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }

    }
}
