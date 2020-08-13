using JWTExample.Domain.Mappers.Interfaces;
using JWTExample.Models.Auth;
using JWTExample.Models.Entities;

namespace JWTExample.Domain.Mappers
{
    public class CreateRequestMapper : IMapper<CreateRequest, Account>
    {
        public Account Map(CreateRequest model)
        {
            return new Account
            {
                Title = model.Title,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
            };
        }
    }
}