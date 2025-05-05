using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Justine.Common.Models;

namespace Justine.Common.Services
{
    public class BasketServices : IServices<Basket>
    {
        public Task AddAsync(Basket entity)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Basket>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Basket> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(Basket entity)
        {
            throw new NotImplementedException();
        }
    }
}
