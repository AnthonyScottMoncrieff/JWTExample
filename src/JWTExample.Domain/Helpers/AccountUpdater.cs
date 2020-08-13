using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Helpers
{
    public class AccountUpdater : IAccountUpdater
    {
        public void Update(UpdateRequest updateRequest, Account account)
        {
            account.Title = updateRequest.Title;
            account.FirstName = updateRequest.FirstName;
            account.LastName = updateRequest.LastName;
            account.Email = updateRequest.Email;
        }
    }
}