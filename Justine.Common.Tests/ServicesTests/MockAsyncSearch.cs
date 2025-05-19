using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.DataModel;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Justine.Common.Tests.ServicesTests
{
    public class MockAsyncSearch<T> : AsyncSearch<T>
    {
        private readonly List<T> _data;

        public MockAsyncSearch(List<T> data)
        {
            _data = data.ToList();
        }

        public override Task<List<T>> GetRemainingAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_data);
        }
    }
}
