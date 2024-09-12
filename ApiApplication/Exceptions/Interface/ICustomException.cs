using System.Net;

namespace ApiApplication.Exceptions.Interface
{
    public interface ICustomException
    {
        string ErrorCode { get; set; }
        HttpStatusCode StatusCode { get; set; }
    }
}
