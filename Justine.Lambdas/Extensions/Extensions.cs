using Justine.Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Justine.Lambdas.Extensions
{
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
            
        }
    }
}
