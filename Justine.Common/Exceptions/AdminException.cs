namespace Justine.Common.Exceptions
{
    // TODO: Add Logging
    public class AdminException : Exception
    {
        public AdminException(string message) : base(message)
        {
        }

        public AdminException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
