using Microsoft.EntityFrameworkCore;

namespace HouseKeeper.Contexts;
public class ApplicationContextFactory : IApplicationContextFactory
{
    private readonly string _connectionString;

    public ApplicationContextFactory(string connectionString)
    {
        _connectionString = connectionString;
    }   

    public Task<ApplicationContext> Create()
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>()
            .UseNpgsql(_connectionString);
        var result = new ApplicationContext(optionsBuilder.Options);
        return Task.FromResult(result);
    }
}
