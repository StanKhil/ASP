using ASP.Data;
using ASP.Data.Entities;
using ASP.Models.Api.Group;
using ASP.Models.Rest;
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
        public RestResponse ExecuteGET()
        {
            var groups = _dataAccessor.GetProductGroups();
            RestResponse response = new();
            response.Meta.ResourceName = "ProductGroups";
            response.Meta.ResourceUrl = "/api/product-group";
            response.Meta.Method = "GET";
            response.Meta.DataType = nameof(ProductGroup);

            response.Status = new RestStatus
            {
                Code = 200,
                IsOk = true,
                Phrase = "OK"
            };
            return response;
        }

        [HttpPost]
        public RestResponse ExecutePOST([FromForm] ApiGroupFormModel formModel)
        {
            RestResponse response = new();

            if (string.IsNullOrEmpty(formModel.Slug))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Slug is required";
            }

            if (_dataAccessor.IsGroupSlugUsed(formModel.Slug))
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Slug is already used";
            }

            if (!string.IsNullOrEmpty(formModel.ParentId))
            {
                if (!Guid.TryParse(formModel.ParentId, out Guid parsedId))
                {
                    response.Status = RestStatus.RestStatus400;
                    response.Data = "ParentId is not a valid GUID";
                }

                if (!_dataAccessor.IsGroupExists(formModel.ParentId))
                {
                    response.Status = RestStatus.RestStatus400;
                    response.Data = "ParentId does not exist";
                }
            }

            string? savedName = null;
            try
            {
                _storageService.TryGetMimeType(formModel.Image.FileName);
                savedName = _storageService.SaveItem(formModel.Image);
            }
            catch (Exception ex)
            {
                response.Status = RestStatus.RestStatus400;
                response.Data = "Image upload failed: " + ex.Message;
            }

            try
            {
                if (savedName != null)
                {
                    _dataAccessor.AddProductGroup(new()
                    {
                        Name = formModel.Name,
                        Description = formModel.Description,
                        Slug = formModel.Slug,
                        ParentId = formModel.ParentId,
                        ImageUrl = savedName
                    });
                }

                response.Status = RestStatus.RestStatus201;
            }
            catch
            {
                response.Status = RestStatus.RestStatus500;
            }
            return response;
        }

        private RestMeta CreateMeta(string method)
        {
            return new RestMeta
            {
                ResourceName = "ProductGroups",
                ResourceUrl = "/api/product-group",
                Method = method,
                DataType = nameof(ProductGroup)
            };
        }
    }
}
