using JWTExample.Domain.Helpers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Helpers
{
    public class AccountUpdater : IAccountUpdater
    {
        public void Update(UpdateRequest updateRequest, Account account)
        {
            account.Title = string.IsNullOrEmpty(updateRequest.Title) ? account.Title : updateRequest.Title;
            account.FirstName = string.IsNullOrEmpty(updateRequest.FirstName) ? account.FirstName : updateRequest.FirstName;
            account.LastName = string.IsNullOrEmpty(updateRequest.LastName) ? account.LastName : updateRequest.LastName;
            account.Email = string.IsNullOrEmpty(updateRequest.Email) ? account.Email : updateRequest.Email; ;
        }
    }
}