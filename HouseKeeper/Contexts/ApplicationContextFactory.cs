namespace HouseKeeper.Contexts;
public class ApplicationContextFactory : IApplicationContextFactory
{
    public Task<ApplicationContext> Create()
    {
        var result = new ApplicationContext();
        return Task.FromResult(result);
    }
}
