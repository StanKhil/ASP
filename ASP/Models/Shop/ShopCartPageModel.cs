using ASP.Data.Entities;

namespace ASP.Models.Shop
{
    public class ShopCartPageModel
    {
        public IEnumerable<CartItem> ActiveCartItems
        { get; set; } = null!;
        public bool IsActive { get; set; } = true;
    }
}
