using ASP.Models.Api.Product;
using ASP.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ASP.Filters;
using ASP.Data;
using ASP.Models.Rest;

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
            RestResponse response = new();
            response.Meta.ResourceName = "Shop Api 'product'";
            response.Meta.ResourceUrl = $"/api/product";
            response.Meta.Method = "POST";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];
            response.Meta.DataType = "string";

            if (string.IsNullOrWhiteSpace(formModel.GroupId) || !_dataAccessor.IsGroupExists(formModel.GroupId))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Invalid or missing GroupId";
            }

            if (string.IsNullOrWhiteSpace(formModel.Name) || formModel.Name.Length < 3 || formModel.Name.Length > 100)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Name must be between 3 and 100 characters";
            }

            if (!string.IsNullOrWhiteSpace(formModel.Description) && formModel.Description.Length > 1000)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Description cannot be longer than 1000 characters";
            }

            if (formModel.Price <= 0)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Price must be positive";
            }

            if (formModel.Stock < 0)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Stock cannot be negative";
            }

            if (formModel.Slug != null)
            {
                if (_dataAccessor.IsProductSlugUsed(formModel.Slug))
                {
                    response.Status = RestStatus.RestStatus400;
                    response.Data = "Slug is already used";
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
                    response.Status = RestStatus.RestStatus400;
                    response.Data = $"Image error: {ex.Message}";
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
                response.Status = RestStatus.RestStatus201;
            }
            catch (Exception e) when (e is ArgumentNullException || e is FormatException)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = e.Message;
            }
            catch
            {
                response.Status = RestStatus.RestStatus500;
            }
            return response;
        }
    }
}
