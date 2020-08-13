using JWTExample.Data;
using JWTExample.Models.Entities;
using JWTExample.Models.Interfaces;
using System.Threading.Tasks;

namespace JWTExample.Domain.Helpers.Interfaces
{
    public interface IAccountServiceHelpers
    {
        Task<Account> getAccount(int id, DataContext dataContext);

        Task<(RefreshToken, Account)> getRefreshToken(string token, DataContext dataContext);

        string generateJwtToken(Account account, IAppSettings appSettings);

        RefreshToken generateRefreshToken(string ipAddress);

        string randomTokenString();
    }
}