using Microsoft.AspNetCore.Mvc;

namespace ASP.Models.Api.Product
{
    public class ApiProductFormModel
    {
        [FromForm(Name = "product-name")]
        public String Name { get; set; } = null!;

        [FromForm(Name = "product-img")]
        public IFormFile Image { get; set; } = null!;
    }
}
