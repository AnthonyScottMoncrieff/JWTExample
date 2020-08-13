using JWTExample.Models.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTExample.Domain.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest model, string ipAddress);

        Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);

        Task RevokeTokenAsync(string token, string ipAddress);

        Task RegisterAsync(RegisterRequest model);

        Task ValidateResetTokenAsync(ValidateResetTokenRequest model);

        Task<IEnumerable<AccountResponse>> GetAllAsync();

        Task<AccountResponse> GetByIdAsync(int id);

        Task<AccountResponse> CreateAsync(CreateRequest model);

        Task<AccountResponse> UpdateAsync(int id, UpdateRequest model);

        Task DeleteAsync(int id);
    }
}