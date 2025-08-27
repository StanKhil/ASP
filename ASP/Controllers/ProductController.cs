using ASP.Models.Api.Product;
using ASP.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASP.Filters;

namespace ASP.Controllers
{
    [Route("api/product")]
    [ApiController]
    [AuthorizationFilter]
    public class ProductController(IStorageService storageService) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;

        [HttpGet]
        public IEnumerable<String> ProductList()
        {
            return ["1", "2", "3", "4"];
        }

        [HttpPost]
        public async Task<object> CreateProduct(ApiProductFormModel model)
        {

            String savedName;
            try
            {
                _storageService.TryGetMimeType(model.Image.FileName);
                savedName = await _storageService.SaveItemAsync(model.Image);
            }
            catch (Exception ex)
            {
                return new { status = "Fail", name = ex.Message };
            }
            return new {status = "OK", name = "CreateProduct" };    
        }
    }
}
