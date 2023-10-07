namespace API.Errors
{   // response of what will send back to the client
    public class ApiException
    {
        public ApiException(int statusCode, string message, string details)
        {
            StatusCode = StatusCode;
            Message = message;
            Details = details;
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}