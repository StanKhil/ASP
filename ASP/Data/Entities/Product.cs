using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.Data.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public Guid? GroupId { get; set; } = null!;
        public String Name { get; set; } = null!;
        public String? Description { get; set; }
        public String? Slug { get; set; }
        public String ImageUrl { get; set; } = null!;
        [Column(TypeName = "decimal(12,2)")]
        public double Price { get; set; }
        public int Stock {  get; set; }
        public DateTime? DeletedAt { get; set; }
        public ProductGroup? Group { get; set; }
    }
}
