using JWTExample.Data;
using JWTExample.Models.Entities;
using JWTExample.Models.Interfaces;
using System.Threading.Tasks;

namespace JWTExample.Domain.Helpers.Interfaces
{
    public interface IAccountServiceHelpers
    {
        Task<Account> GetAccountAsync(int id, DataContext dataContext);

        Task<(RefreshToken, Account)> GetRefreshTokenAsync(string token, DataContext dataContext);

        string GenerateJwtToken(Account account, IAppSettings appSettings);

        RefreshToken GenerateRefreshToken(string ipAddress);

        string RandomTokenString();
    }
}