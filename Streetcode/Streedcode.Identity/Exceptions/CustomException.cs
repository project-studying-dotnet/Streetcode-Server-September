namespace Streetcode.Identity.Exceptions
{
    public class CustomException : Exception
    {
        public int StatusCode { get; }

        public CustomException() { }

        public CustomException(string message, int statusCode) : base(message)
        {
            StatusCode = statusCode;
        }

        public CustomException(string message, int statusCode, Exception innerException)
        : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
