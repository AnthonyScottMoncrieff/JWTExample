using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Mappers
{
    public class AccountResponseMapper : IMapper<Account, AccountResponse>
    {
        public AccountResponse Map(Account model)
        {
            return new AccountResponse
            {
                Id = model.Id,
                Created = model.Created,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Role = model.Role.ToString(),
                Title = model.Title,
                Updated = model.Updated
            };
        }
    }
}