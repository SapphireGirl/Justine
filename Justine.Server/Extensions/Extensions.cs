using Justine.Common.Models;
using Justine.Common.Services;

namespace Justine.Server.Extensions;

public static class ServiceExtensions
{
    public static void RegisterRepos(this IServiceCollection collection)
    {
        collection.AddTransient<IServices<Product>, ProductServices>();
        collection.AddTransient<IServices<Basket>, BasketServices>();
        collection.AddTransient<IServices<Order>, OrderServices>();

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