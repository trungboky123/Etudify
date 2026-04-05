namespace back_end.Dto.Response;

public class ApiExceptionResponse : Exception
{
    public int StatusCode { get; set; }
    public object Errors { get; set; }

    public ApiExceptionResponse(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}