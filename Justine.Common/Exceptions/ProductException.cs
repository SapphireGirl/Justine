using System.Net;

namespace Justine.Common.Exceptions
{
    public class ProductException : AppException
    {
        public ProductException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public ProductException(string message, Exception innerException) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
    
}
