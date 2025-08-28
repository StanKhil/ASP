using ASP.Data;
using ASP.Data.Entities;
using ASP.Models.Api.Group;
using ASP.Services.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ASP.Controllers.Api
{
    [Route("api/product-group")]
    [ApiController]
    public class ProductGroupController(DataAccessor dataAccessor,
        IStorageService storageService) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly IStorageService _storageService = storageService;

        private object AnyRequest()
        {
            string methodName = "Execute" + HttpContext.Request.Method;
            var type = GetType();
            var action = type.GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (HttpContext.Request.Method == "GET")
            {
                return action.Invoke(this, null);
            }
            if (action == null)
            {
                return new
                {
                    status = "404",
                    message = "Not Found"
                };

            }
            bool isAuthenticated = HttpContext.User.Identity?.IsAuthenticated ?? false;
            if (!isAuthenticated)
            {
                return new { status = "401", message = "Unauthorized" };
            }

            return action.Invoke(this, null);


            //return HttpContext.Request.Method switch
            //{
            //    "POST" => this.GetList(),
            //    "PUT" => this.GetList(),
            //    _ => new
            //        {
            //            status = "404",
            //            message = "Not Found"
            //        }
            //};
        }

        [HttpGet]
        public IEnumerable<ProductGroup> ExecuteGET()
        {
            return _dataAccessor.GetProductGroups();
        }

        [HttpPost]
        public object ExecutePOST(ApiGroupFormModel formModel)
        {
            if (string.IsNullOrEmpty(formModel.Slug))
            {
                return new { status = 400, name = "Slug could not be empty" };
            }
            if (_dataAccessor.IsGroupSlugUsed(formModel.Slug))
            {
                return new { status = 409, name = "Slug is used by other group" };
            }
            if (!string.IsNullOrEmpty(formModel.ParentId))
            {
                if (!Guid.TryParse(formModel.ParentId, out Guid parsedId))
                {
                    return new { status = 400, name = "ParentId must be a valid UUID" };
                }

                if (!_dataAccessor.IsGroupExists(formModel.ParentId))
                {
                    return new { status = 404, name = "Parent group does not exist" };
                }
            }

            string savedName;
            try
            {
                _storageService.TryGetMimeType(formModel.Image.FileName);
                savedName = _storageService.SaveItem(formModel.Image);
            }
            catch (Exception ex)
            {
                return new { status = 400, name = ex.Message };
            }

            try
            {
                _dataAccessor.AddProductGroup(new()
                {
                    Name = formModel.Name,
                    Description = formModel.Description,
                    Slug = formModel.Slug,
                    ParentId = formModel.ParentId,
                    ImageUrl = savedName
                });
                return new { status = 201, name = "Created" };
            }
            catch
            {
                return new { status = 500, name = "Error" };
            }
        }
    }
}
