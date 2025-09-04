using ASP.Data;
using ASP.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataAccessor dataAccessor) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;

        [HttpPost("{id}")]
        public Object AddToCart([FromRoute] String id)
        {
            RestResponse response = new();
            response.Meta.ResourceName = "Shop Api 'user Cart'";
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Method = "POST";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];

            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String? userId = HttpContext.User.Claims
                    .FirstOrDefault (c => c.Type == ClaimTypes.PrimarySid)?.Value;
                if(userId == null)
                {
                    response.Status = RestStatus.RestStatus403;
                    response.Data = "PrimarySid not found";
                    response.Meta.DataType = "string";
                }

                else try
                {
                    _dataAccessor.AddToCart(userId, id);
                }
                catch (Exception e) when (e is ArgumentNullException || e is FormatException)
                {
                    response.Status = RestStatus.RestStatus400;
                    response.Data = e.Message;
                        response.Meta.DataType = "string";
                    }
                catch
                {
                    response.Status = RestStatus.RestStatus500;
                }
            }
            else
            {
                response.Status = RestStatus.RestStatus401;
            }
            return response;
        }
    }
}
