using System.Net;

namespace Justine.Common.Exceptions
{
    public abstract class AppException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        protected AppException(string msg, HttpStatusCode statusCode = HttpStatusCode.InternalServerError) : base()
        {
            StatusCode = statusCode;

        }
    }
}
