namespace ASP.Models.Home
{
    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }
        public int Amount { get; set; }
    }

    public class HomeDemoPageModel
    {
        public Product[] Products { get; set; }
    }
}
