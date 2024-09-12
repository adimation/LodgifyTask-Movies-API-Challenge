using ApiApplication.Exceptions.Interface;
using System;
using System.Net;

namespace ApiApplication.Exceptions
{
    public class MoviesApiClientException : Exception, ICustomException
    {
        public string ErrorCode { get; set; }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;

        public MoviesApiClientException() { }

        public MoviesApiClientException(string message) : base(message) { }
        public MoviesApiClientException(string message, string errorCode) : base(message) { }
        public MoviesApiClientException(string message, Exception innerException) : base(message, innerException) { }
        public MoviesApiClientException(string message, string errorCode, Exception innerException) : base(message, innerException) 
        {
            ErrorCode = errorCode;
        }
    }
}