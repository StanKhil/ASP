using ASP.Data.Entities;

namespace ASP.Models.Shop
{
    public class ShopIndexPageModel
    {
        public IEnumerable<ProductGroup> ProductGroups { get; set; } = [];
    }
}
