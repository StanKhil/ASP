using ASP.Data;
using ASP.Models.Rest;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ASP.Controllers.Api
{
    [Route("api/cart")]
    [ApiController]
    public class CartController(DataAccessor dataAccessor,
        ILogger<CartController> logger) : ControllerBase
    {
        private readonly DataAccessor _dataAccessor = dataAccessor;
        private readonly ILogger<CartController> _logger = logger;

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
                    .FirstOrDefault(c => c.Type == ClaimTypes.PrimarySid)?.Value;
                if (userId == null)
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
                    catch (Exception e)
                    {
                        response.Status = RestStatus.RestStatus500;
                        _logger.LogError(e, "Error adding product {ProductId} to cart for user {UserId}", id, userId);
                    }
            }
            else
            {
                response.Status = RestStatus.RestStatus401;
            }
            return response;
        }

        [HttpPatch("{id}")]
        public RestResponse ChangeCart(String id, int increment)
        {
            RestResponse response = new RestResponse();
            response.Meta.ResourceName = "Shop Api 'user Cart'";
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Method = "PUT";
            response.Meta.Manipulations = ["POST", "PATCH", "DELETE"];
            response.Data = increment;

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
                        _dataAccessor.ModifyCart(userId, id, increment);
                    }
                    catch(ArgumentOutOfRangeException e)
                    {
                        response.Status = RestStatus.RestStatus409;
                        response.Data = "Increment too large (out of stock)";
                        response.Meta.DataType = "string";
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
                        _logger.LogError(e, "Error modifying product {ProductId} to cart for user {UserId}", id, userId);
                    }
            }
            else
            {
                response.Status = RestStatus.RestStatus401;
            }

            return response;
        }

        [HttpDelete("{id}")]
        public RestResponse RemoveFromCart([FromRoute] String id)
        {
            RestResponse response = new();
            response.Meta.ResourceName = "Shop Api 'user Cart'";
            response.Meta.ResourceUrl = $"/api/cart/{id}";
            response.Meta.Method = "DELETE";
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
                        _dataAccessor.RemoveFromCart(userId, id);
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
                        _logger.LogError(e, "Error removing product {ProductId} from cart for user {UserId}", id, userId);
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
