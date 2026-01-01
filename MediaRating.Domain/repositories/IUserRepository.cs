using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaRatings.Domain.repositories
{
    public interface IUserRepository
    {
        Task<UserAccount?> FindByUsernameAsync(string username);
        Task<UserAccount?> GetByIdAsync(int userId);
        Task<UserAccount> CreateAsync(string username, string passwordPlain);
        Task<bool> ValidateCredentialsAsync(string username, string passwordPlain);
        Task<List<UserAccount>> GetAllUsersAsync();
    }
}
