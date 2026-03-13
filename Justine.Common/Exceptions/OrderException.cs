using System.Net;

namespace Justine.Common.Exceptions
{
    public class OrderException : AppException
    {
        public OrderException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public OrderException(string message, Exception innerException) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
