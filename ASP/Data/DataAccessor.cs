using ASP.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ASP.Data
{
    public class DataAccessor(DataContext dataContext)
    {
        private readonly DataContext _dataContext = dataContext;

        public UserAccess? GerUserAccessByLogin(String userLogin, bool isEditable = false)
        {
            IQueryable<UserAccess> source = _dataContext.UserAccesses
                .Include(ua => ua.UserData)
                .Include(ua => ua.UserRole);
            if (isEditable) source = source.AsNoTracking();
            return source.FirstOrDefault(ua => ua.Login == userLogin);
        }

        public void UpdateUserData(UserData)
    }
}
