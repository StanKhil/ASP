using ASP.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP.Data
{
    public class DataAccessor(DataContext dataContext, ILogger<DataAccessor> logger)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly ILogger<DataAccessor> _logger = logger;

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
    }
}
