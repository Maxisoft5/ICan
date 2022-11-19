using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ICan.Data.Context
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=utg;Username=postgres;Password=Passw0rd");
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }

        //private static void SetSettings(IServiceCollection services, IConfiguration configuration)
        //{
        //    services.AddDbContext<UtgDocContext>(options =>
        //    {
        //        options.UseNpgsql(configuration.GetConnectionString("UTGDocDatabase"));
        //    });
        //}
    }
}
