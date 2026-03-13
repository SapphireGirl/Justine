using System.Net;

namespace Justine.Common.Exceptions
{
    public class AdminException : AppException
    {
        public AdminException(string message) : base(message, HttpStatusCode.BadRequest)
        {
        }

        public AdminException(string message, Exception innerException) : base(message, HttpStatusCode.BadRequest)
        {
        }
    }
}
