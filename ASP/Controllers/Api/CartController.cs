using ASP.Data;
using ASP.Filters;
using ASP.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ASP.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    [RestFilter(Name: "Shop API 'user cart'")]
    public class CartController(DataAccessor dataAccessor,
        ILogger<CartController> logger) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<CartController> _logger = logger;
        private RestResponse response = null!;


        [HttpGet]
        public RestResponse GetActiveCart()
        {
            response.Meta.ResourceUrl = $"/api/cart/";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE", "PUT"];
            ExecuteAuthority((userId) =>
            {
                response.Data = _dataAccessor.GetActiveCart(userId);
                response.Meta.DataType = "object";
            }, nameof(GetActiveCart));
            return response;
        }

        [HttpPost("{id}")]
        public Object AddToCart([FromRoute] String id)
        {
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];

            ExecuteAuthority((userId) => _dataAccessor.AddToCart(userId, id)
                , nameof(AddToCart));
            return response;
        }

        [HttpPatch("{id}")]
        public RestResponse ChangeCart(String id, int increment)
        {
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];
            response.Data = increment;

            ExecuteAuthority((userId) =>
                _dataAccessor.ModifyCart(userId, id, increment)
                , nameof(ChangeCart));
            return response;
        }

        [HttpDelete("{id}")]
        public RestResponse RemoveFromCart([FromRoute] String id)
        {
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];

            ExecuteAuthority((userId) =>
                _dataAccessor.RemoveFromCart(userId, id)
                , nameof(RemoveFromCart));
            return response;
        }

        [HttpDelete]
        public RestResponse DiscardCart()
        {
            response.Meta.ResourceUrl = $"/api/cart/";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];

            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String? userId = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                if (userId == null)
                {
                    response.Status = RestStatus.RestStatus403;
                    response.Data = "PrimarySid not found";
                    response.Meta.DataType = "string";
                }

                else try
                    {
                        _dataAccessor.DiscardActiveCart(userId);
                    }
                    catch (Exception e) when (e is ArgumentNullException || e is FormatException)
                    {
                        response.Status = RestStatus.RestStatus400;
                        response.Data = e.Message;
                        response.Meta.DataType = "string";
                    }
                    catch (Exception e)
                    {
                        response.Status = RestStatus.RestStatus500;
                        _logger.LogError(e, "Discard error: " + e.Message);
                    }
            }
            else
            {
                response.Status = RestStatus.RestStatus401;
            }
            return response;
        }

        [HttpPut]
        public RestResponse CheckoutCart()
        {
            RestResponse response = new RestResponse();
            response.Meta.ResourceUrl = $"/api/cart/";
            response.Meta.Manipulations = ["PUT", "DELETE"];

            ExecuteAuthority(
                _dataAccessor.CheckoutActiveCart
                , nameof(CheckoutCart));
            return response;
        }

        [HttpPost("repeat/{id}")]
        public RestResponse RepeatCart(String id)
        {
            response.Data = id;
            return response;
        }
        private void ExecuteAuthority(Action<String> action, String callerName)
        {
            if (HttpContext.User.Identity?.IsAuthenticated ?? false)
            {
                String? userId = HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                if (userId == null)
                {
                    response.Status = RestStatus.RestStatus403;
                    response.Data = "PrimarySid not found";
                    response.Meta.DataType = "string";
                }

                else try
                    {
                        action(userId);
                    }
                    catch (Exception e) when (e is ArgumentNullException || e is FormatException)
                    {
                        response.Status = RestStatus.RestStatus400;
                        response.Data = e.Message;
                        response.Meta.DataType = "string";
                    }
                    catch (Exception e)
                    {
                        response.Status = RestStatus.RestStatus500;
                        _logger.LogError($"{callerName} " + e.Message);
                    }
            }
            else
            {
                response.Status = RestStatus.RestStatus401;
            }
        }
    }
}
