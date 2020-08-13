using JWTExample.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JWTExample.Data
{
    public class DataContext : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DataContext(DbContextOptions<DataContext> dbContextOptions) : base(dbContextOptions)
        {
        }
    }
}