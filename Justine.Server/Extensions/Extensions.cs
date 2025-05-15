using Justine.Common.Models;
using Justine.Common.Services;

namespace Justine.Server.Extensions;

public static class ServiceExtensions
{
    public static void RegisterRepos(this IServiceCollection collection)
    {
        collection.AddTransient<IProductServices, ProductServices>();
        collection.AddTransient<IBasketServices, BasketServices>();
        collection.AddTransient<IOrderServices, OrderServices>();

    }

    public static void SetupDbContext(this IServiceCollection collection)
    {

    }

    public static void RegisterLogging(this IServiceCollection collection)
    {
        //collection.AddTransient<ISFLogger, SFLogger>();
        //collection.AddSingleton<DapperContext>();
        //collection.AddTransient<IRepository<Home>, HomeRepository>();
    }
}