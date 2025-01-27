namespace HouseKeeper.Contexts;
public interface IApplicationContextFactory
{
    Task<ApplicationContext> Create();
}
