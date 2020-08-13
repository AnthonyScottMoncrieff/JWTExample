using JWTExample.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTExample.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }
    }
}