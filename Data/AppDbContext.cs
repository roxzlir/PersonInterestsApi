using Microsoft.EntityFrameworkCore;
using PersonInterestsApi.Models;

namespace PersonInterestsApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Person> Persons { get; set; }
        public DbSet<Interest> Interests { get; set; }
        public DbSet<PersonInterest> PersonInterests { get; set; }
        public DbSet<Link> Links { get; set; }
    }
}
