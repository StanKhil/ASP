using System.ComponentModel.DataAnnotations.Schema;

namespace ASP.Data.Entities
{
    public class Cart
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        [Column(TypeName = "decimal(15,2)")]
        public decimal Price { get; set; }
        public Guid? DiscountItemId { get; set; }
        public List<CartItem> CartItems { get; set; } = [];
        public UserData User { get; set; } = null!;
    }
}
