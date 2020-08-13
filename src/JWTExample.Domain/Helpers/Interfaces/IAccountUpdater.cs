using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Helpers.Interfaces
{
    public interface IAccountUpdater
    {
        void Update(UpdateRequest updateRequest, Account account);
    }
}