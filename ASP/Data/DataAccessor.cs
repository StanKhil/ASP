using ASP.Data.Entities;
using ASP.Models.Api.Group;
using ASP.Models.Api.Product;
using Microsoft.EntityFrameworkCore;

namespace ASP.Data
{
    public class DataAccessor(DataContext dataContext, ILogger<DataAccessor> logger)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly ILogger<DataAccessor> _logger = logger;

        public IEnumerable<ProductGroup> GetProductGroups()
        {
            return _dataContext.ProductGroups
                .Where(g => g.DeletedAt == null)
                .AsNoTracking()
                .AsEnumerable();
        }
        public UserAccess? GerUserAccessByLogin(String userLogin, bool isEditable = false)
        {
            IQueryable<UserAccess> source = _dataContext.UserAccesses
                .Include(ua => ua.UserData)
                .Include(ua => ua.UserRole);
            if (isEditable) source = source.AsNoTracking();
            return source
                .FirstOrDefault(ua => ua.Login == userLogin 
                && ua.UserData.DeletedAt == null);
        }

        //public void UpdateUserData(UserData)

        public async Task<bool> DeleteUserAsync(String userLogin)
        {
            UserAccess? ua = await _dataContext
                .UserAccesses
                .Include(ua => ua.UserData)
                .FirstOrDefaultAsync(ua => ua.Login == userLogin);
            if(ua == null)
            {
                return false;
            }
            ua.UserData.Name = "";
            ua.UserData.Email = "";
            ua.UserData.Birthdate = null;
            ua.UserData.DeletedAt = DateTime.UtcNow;
            try
            {
                await _dataContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserLogin}", userLogin);
                return false;
            }
        }

        public bool IsGroupSlugUsed(String slug)
        {
            return _dataContext.ProductGroups
                .Any(g => g.Slug == slug);
        }

        public bool IsProductSlugUsed(String slug)
        {
            return _dataContext.Products
                .Any(g => g.Slug == slug);
        }

        public bool IsGroupExists(String id)
        {
            return _dataContext
                .ProductGroups
                .Any(g => g.Id == Guid.Parse(id));
        }

        public void AddProductGroup(ApiGroupDataModel model)
        {
            _dataContext.ProductGroups.Add(new()
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                Description = model.Description,
                Slug = model.Slug,
                ImageUrl = model.ImageUrl,
                ParentId = model.ParentId == null ? null : Guid.Parse(model.ParentId),
                DeletedAt = null,
            });
            try
            {
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddProductGroup: {ex}", ex.Message);
                throw;
            }
        }

        public ProductGroup? GetProductGroupBySlug(String slug)
        {
            return _dataContext.ProductGroups
                .Include(g => g.Products)
                .AsNoTracking()
                .FirstOrDefault(g => g.Slug == slug && g.DeletedAt == null);
        }

        public Product? GetProductBySlug(String slug)
        {
            return _dataContext.Products
                .AsNoTracking()
                .FirstOrDefault(p => (p.Slug == slug || p.Id.ToString() == slug) && p.DeletedAt == null);
        }

        public void AddProduct(ApiProductDataModel model)
        {
            Guid groupId;
            try
            {
                groupId = Guid.Parse(model.GroupId);
            }catch { throw; }
            _dataContext.Products.Add(new()
            {
                Id = Guid.NewGuid(),
                GroupId = groupId,
                Name = model.Name,
                Description = model.Description,
                Slug = model.Slug,
                ImageUrl = model.ImageUrl,
                Price = model.Price,
                Stock = model.Stock,
                DeletedAt = null,
            });
            try
            {
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError("AddProductGroup: {ex}", ex.Message);
                throw;
            }
        }

        public void AddToCart(String userId, String id)
        {
            Guid userGuid = Guid.Parse(userId);
            Guid productGuid = Guid.Parse(id);

            var user = _dataContext.Users
                .Find(userGuid) ?? throw new ArgumentNullException("User not found", nameof(userId));

            var product = _dataContext.Products
                .Find(productGuid) ?? throw new ArgumentNullException("Product not found", nameof(id));

            var cart = _dataContext.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == userGuid && c.PaidAt == null && c.DeletedAt == null);

            if (cart == null)
            {
                cart = new Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userGuid,
                    CreatedAt = DateTime.UtcNow,
                    Price = 0,
                };
                _dataContext.Carts.Add(cart);
            }

            CartItem? cartItem = cart.CartItems
                .FirstOrDefault(ci => ci.ProductId == productGuid);

            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = productGuid,
                    Quantity = 1,
                    Price = product.Price,
                };
                //cart.CartItems.Add(cartItem);
                _dataContext.CartItems.Add(cartItem);
                cart.Price += product.Price;
            }
            else
            {
                cartItem.Quantity += 1;
                cartItem.Price += product.Price;
                cart.Price += product.Price;
            }
            _dataContext.SaveChanges();
        }

        public IEnumerable<CartItem> GetActiveCartItems(String userId)
        {
            Guid userGuid = Guid.Parse(userId);

            var user = _dataContext.Users
                .Find(userGuid) ?? throw new ArgumentNullException("User not found", nameof(userId));

            var cart = _dataContext
                .Carts
                .AsNoTracking()
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == userGuid && c.PaidAt == null && c.DeletedAt == null);

            return cart?.CartItems ?? Enumerable.Empty<CartItem>();
        }
    }
}
