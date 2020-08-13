using JWTExample.Models.Auth;
using System.Collections.Generic;

namespace JWTExample.Domain.Services.Interfaces
{
    public interface IAccountService
    {
        AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress);

        AuthenticateResponse RefreshToken(string token, string ipAddress);

        void RevokeToken(string token, string ipAddress);

        void Register(RegisterRequest model, string origin);

        void ValidateResetToken(ValidateResetTokenRequest model);

        IEnumerable<AccountResponse> GetAll();

        AccountResponse GetById(int id);

        AccountResponse Create(CreateRequest model);

        AccountResponse Update(int id, UpdateRequest model);

        void Delete(int id);
    }
}