namespace ApiApplication.DTOs.Abstract
{
    public class ApiResponseDTO<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public T Payload { get; set; }
    }
}
