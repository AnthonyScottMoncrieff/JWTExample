using Microsoft.AspNetCore.Http;

namespace JWTExample.Domain.Helpers.Interfaces
{
    public interface IAccountControllerHelpers
    {
        void SetTokenCookie(string token, HttpResponse response);

        string GetIpAddress(HttpResponse response, HttpContext httpContext);
    }
}