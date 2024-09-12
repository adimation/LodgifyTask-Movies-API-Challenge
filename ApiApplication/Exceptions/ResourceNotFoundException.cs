using ApiApplication.Exceptions.Interface;
using System;
using System.Net;

namespace ApiApplication.Exceptions
{
    public class ResourceNotFoundException : Exception, ICustomException
    {
        public string ErrorCode { get; set; }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.NotFound;

        public ResourceNotFoundException() { }

        public ResourceNotFoundException(string message) : base(message) { }
        public ResourceNotFoundException(string message, string errorCode) : base(message) {
            ErrorCode = errorCode;
        }
        public ResourceNotFoundException(string message, Exception innerException) : base(message, innerException) { }
        public ResourceNotFoundException(string message, string errorCode, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
