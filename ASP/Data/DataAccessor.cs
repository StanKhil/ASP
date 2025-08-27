using ASP.Data.Entities;
using ASP.Models.Api.Group;
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

        public bool IsGroupExists(String id)
        {
            return _dataContext
                .ProductGroups
                .Any(g => g.Id.ToString() == id);
                
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
    }
}
