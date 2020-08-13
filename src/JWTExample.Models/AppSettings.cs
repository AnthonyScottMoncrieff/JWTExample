using JWTExample.Models.Interfaces;
using Microsoft.Extensions.Configuration;

namespace JWTExample.Models
{
    public class AppSettings : IAppSettings
    {
        public AppSettings(IConfiguration Configuration)
        {
            Secret = Configuration["JWT:Secret"];
        }

        public string Secret { get; set; }
    }
}