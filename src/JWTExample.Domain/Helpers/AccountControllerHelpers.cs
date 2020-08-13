using JWTExample.Domain.Helpers.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace JWTExample.Domain.Helpers
{
    public class AccountControllerHelpers : IAccountControllerHelpers
    {
        public void SetTokenCookie(string token, HttpResponse response)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7)
            };
            response.Cookies.Append("refreshToken", token, cookieOptions);
        }

        public string GetIpAddress(HttpResponse response, HttpContext httpContext)
        {
            if (response.Headers.ContainsKey("X-Forwarded-For"))
                return response.Headers["X-Forwarded-For"];
            else
                return httpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}