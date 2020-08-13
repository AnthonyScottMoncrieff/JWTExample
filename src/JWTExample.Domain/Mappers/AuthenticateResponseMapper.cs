using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Mappers
{
    public class AuthenticateResponseMapper : IMapper<Account, AuthenticateResponse>
    {
        public AuthenticateResponse Map(Account model)
        {
            return new AuthenticateResponse
            {
                Id = model.Id,
                Title = model.Title,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Created = model.Created,
                Updated = model.Updated
            };
        }
    }
}