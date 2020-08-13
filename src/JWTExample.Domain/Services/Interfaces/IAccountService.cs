using JWTExample.Models.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JWTExample.Domain.Services.Interfaces
{
    public interface IAccountService
    {
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest model, string ipAddress);

        Task<AuthenticateResponse> RefreshToken(string token, string ipAddress);

        Task RevokeToken(string token, string ipAddress);

        Task Register(RegisterRequest model);

        Task ValidateResetToken(ValidateResetTokenRequest model);

        Task<IEnumerable<AccountResponse>> GetAll();

        Task<AccountResponse> GetById(int id);

        Task<AccountResponse> Create(CreateRequest model);

        Task<AccountResponse> Update(int id, UpdateRequest model);

        Task Delete(int id);
    }
}