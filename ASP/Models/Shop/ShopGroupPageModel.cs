using ASP.Data.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace ASP.Models.Shop
{
    public class ShopGroupPageModel
    {
        public ProductGroup? ProductGroup { get; set; } = null!;
        public List<ProductGroup> ProductGroups { get; set; } = new();
    }
}
