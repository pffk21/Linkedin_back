using Microsoft.EntityFrameworkCore;
using Project_X_Data.Data;
using Project_X_Data.Data.Entities;
using Project_X_Data.Models.LogInOut;
using Project_X_Data.Services.Kdf;

namespace Project_X_Data.Data
{
    public class DataAccessor(DataContext dataContext, IKdfService kdfService)
    {
        private readonly DataContext _dataContext = dataContext;
        private readonly IKdfService _kdfService = kdfService;
        public virtual UserAccess? Authenticate(String login, String password)
        {
            var userAccess = _dataContext
                .UserAccesses
                .AsNoTracking()
                .Include(ua => ua.User)
                .Include(ua => ua.Role)
                .FirstOrDefault(ua => ua.Login == login);
            if (userAccess == null)
            {
                return null;
            }
            String dk = _kdfService.Dk(password, userAccess.Salt);
            if (dk != userAccess.Dk)
            {
                return null;
            }
            return userAccess;
        }

        public virtual void SetRole()
        {
            if (!_dataContext.UserRoles.Any())
            {
                _dataContext.UserRoles.AddRange(
                    new UserRole { Id = "Admin", Description = "Administrator", CanDelete = true, CanRead = true, CanUpdate = true, CanWrite = true },
                    new UserRole { Id = "User", Description = "User", CanDelete = false, CanRead = true, CanUpdate = true, CanWrite = true },
                    new UserRole { Id = "Guest", Description = "Guest", CanDelete = false, CanRead = true, CanUpdate = false, CanWrite = false }
                );
                _dataContext.SaveChanges();
            }
        }


        //public IEnumerable<ProductGroup> GetProductGroups()
        //{
        //    return _dataContext
        //        .ProductGroups
        //        .Where(g => g.DeletedAt == null)
        //        .AsEnumerable();
        //}

        public virtual User? GetUserByEmail(string email)
        {
            return _dataContext
                .Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);
        }

        public virtual bool LoginCheck(string email, string password)
        {
            var user = _dataContext
                .Users
                .AsNoTracking()
                .FirstOrDefault(u => u.Email == email);

            var userAccess = _dataContext
                .UserAccesses
                .FirstOrDefault(ua => ua.UserId == user.Id);

            string hash = _kdfService.Dk(password, userAccess.Salt);

            return hash == userAccess.Dk;

        }

        
    }
}
