using System.Net;

namespace Justine.Common.Exceptions
{
    public class BasketException : AppException
    {
        public BasketException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public BasketException(string message, Exception innerException) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
