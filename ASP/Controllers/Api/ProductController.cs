using ASP.Models.Api.Product;
using ASP.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASP.Filters;
using ASP.Data;

namespace ASP.Controllers.Api
{
    [Route("api/product")]
    [ApiController]
    [AuthorizationFilter]
    public class ProductController(IStorageService storageService,
        DataAccessor dataAccessor) : ControllerBase
    {
        private readonly IStorageService _storageService = storageService;
        private readonly DataAccessor _dataAccessor = dataAccessor;


        [HttpGet]
        public IEnumerable<string> ProductList()
        {
            return ["1", "2", "3", "4"];
        }

        [HttpPost]
        public async Task<object> CreateProduct(ApiProductFormModel formModel)
        {
            if (string.IsNullOrWhiteSpace(formModel.GroupId) || !_dataAccessor.IsGroupExists(formModel.GroupId))
            {
                return new { status = 400, name = "Invalid or missing GroupId" };
            }

            if (string.IsNullOrWhiteSpace(formModel.Name) || formModel.Name.Length < 3 || formModel.Name.Length > 100)
            {
                return new { status = 400, name = "Invalid product name" };
            }

            if (!string.IsNullOrWhiteSpace(formModel.Description) && formModel.Description.Length > 1000)
            {
                return new { status = 400, name = "Description too long" };
            }

            if (formModel.Price <= 0)
            {
                return new { status = 400, name = "Price must be greater than 0" };
            }

            if (formModel.Stock < 0)
            {
                return new { status = 400, name = "Stock cannot be negative" };
            }

            if (formModel.Slug != null)
            {
                if (_dataAccessor.IsProductSlugUsed(formModel.Slug))
                {
                    return new { status = 409, name = "Slug is used by other product" };
                }
            }
            
            string? savedName = null;
            if (formModel.Name != null)
            {
                try
                {
                    _storageService.TryGetMimeType(formModel.Image.FileName);
                    savedName = await _storageService.SaveItemAsync(formModel.Image);
                }
                catch (Exception ex)
                {
                    return new { status = 400, name = ex.Message };
                }
            }
            try
            {
                _dataAccessor.AddProduct(new()
                {
                    GroupId = formModel.GroupId,
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Price = formModel.Price,
                    Stock = formModel.Stock,
                    ImageUrl = savedName,
                    Slug = formModel.Slug,

                });
                return new { status = 201, name = $"{formModel.Name} created" };
            }
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                return new { status = 400, name = e.Message };
            }
            catch
            {
                return new { status = 500, name = "Error" };
            }
            
        }
    }
}
