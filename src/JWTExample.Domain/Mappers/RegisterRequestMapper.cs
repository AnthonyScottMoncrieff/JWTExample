using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Mappers
{
    public class RegisterRequestMapper : IMapper<RegisterRequest, Account>
    {
        public Account Map(RegisterRequest model)
        {
            return new Account
            {
                Title = model.Title,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                AcceptTerms = model.AcceptTerms
            };
        }
    }
}