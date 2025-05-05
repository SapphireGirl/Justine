using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Justine.Common.Models;

namespace Justine.Common.Services
{
    public class OrderServices : IServices<Order>
    {
        public OrderServices()
        {
        }

        public Task AddAsync(Order entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Order> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Order entity)
        {
            throw new NotImplementedException();
        }

        //public IOrderedEnumerable<Order> CreateOrder(IEnumerable<Order> orders)
        //{
        //    return orders.OrderBy(o => o.OrderDate);
        //}

        //public IOrderedEnumerable<Order> CreateOrder(IEnumerable<Order> orders, IComparer<Order> comparer)
        //{
        //    return orders.OrderBy(o => o.OrderDate, comparer);
        //}
    }
    
}
