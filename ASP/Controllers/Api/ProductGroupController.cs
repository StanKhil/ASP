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

            return new RestResponse
            {
                Status = new RestStatus
                {
                    Code = 200,
                    IsOk = true,
                    Phrase = "OK"
                },
                Meta = new RestMeta
                {
                    ResourceName = "ProductGroups",
                    ResourceUrl = "/api/product-group",
                    Method = "GET",
                    DataType = nameof(ProductGroup)
                },
                Data = groups
            };
        }

        [HttpPost]
        public RestResponse ExecutePOST([FromForm] ApiGroupFormModel formModel)
        {
            if (string.IsNullOrEmpty(formModel.Slug))
            {
                return new RestResponse { Status = RestStatus.RestStatus400, Meta = CreateMeta("POST"), Data = "Slug could not be empty" };
            }

            if (_dataAccessor.IsGroupSlugUsed(formModel.Slug))
            {
                return new RestResponse { Status = new RestStatus { Code = 409, IsOk = false, Phrase = "Conflict" }, Meta = CreateMeta("POST"), Data = "Slug is used by other group" };
            }

            if (!string.IsNullOrEmpty(formModel.ParentId))
            {
                if (!Guid.TryParse(formModel.ParentId, out Guid parsedId))
                {
                    return new RestResponse { Status = RestStatus.RestStatus400, Meta = CreateMeta("POST"), Data = "ParentId must be a valid UUID" };
                }

                if (!_dataAccessor.IsGroupExists(formModel.ParentId))
                {
                    return new RestResponse { Status = new RestStatus { Code = 404, IsOk = false, Phrase = "Not Found" }, Meta = CreateMeta("POST"), Data = "Parent group does not exist" };
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
                return new RestResponse { Status = RestStatus.RestStatus400, Meta = CreateMeta("POST"), Data = ex.Message };
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

                return new RestResponse { Status = RestStatus.RestStatus201, Meta = CreateMeta("POST"), Data = "Created" };
            }
            catch
            {
                return new RestResponse { Status = RestStatus.RestStatus500, Meta = CreateMeta("POST"), Data = "Internal Error" };
            }
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
