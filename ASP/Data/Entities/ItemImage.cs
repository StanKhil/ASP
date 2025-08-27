namespace ASP.Data.Entities
{
    public class ItemImage
    {
        public Guid ItemId { get; set; }
        public String ImageUrl { get; set; } = null!;
        public int Order { get; set; }
    }
}
